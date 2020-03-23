using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colorful;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor
    {
        public const string BodyTemplate = @"_template\index.2.html";

        internal NavItem BaseItem = new NavItem("User Guide", "guide.html") { UID = "guide" };

        public string CardTemplate = @"_template\card.frag";

        private List<string> MDs = new List<string>();

        private HtmlMinifier minifier = new HtmlMinifier(new HtmlMinificationSettings(true)
        {
            MinifyInlineJsCode = true,
            WhitespaceMinificationMode = WhitespaceMinificationMode.Medium
        });

        private string navHtml = "";
        private string optHtml = "";
        private NavItem navs;
        private MarkdownPipeline p;
        private string raw_html;

        public Processor(string rootPath)
        {
            Nav.RootPath = rootPath;
            raw_html = File.ReadAllText(Nav.RootPath + BodyTemplate);
        }

        public string ScreenshotPath { get; set; } = @"screenshots\";
        public string TutorialsPath { get; set; } = @"tutorials\";

        public bool ProcessProceduralFiles { get; set; } = false;

        public bool ProcessExampleFiles { get; set; } = false;

        public bool ProcessTutorialFiles { get; set; } = false;

        internal string src { get; set; } = @"source\";

        internal string dst { get; set; } = @"docs\";

        public string MainNavHTML { get; set; }

        internal void Process()
        {
            navs = ParseDirectory(Nav.RootPath + src);
            p = new MarkdownPipelineBuilder().UsePipeTables(new PipeTableOptions { RequireHeaderSeparator = true })
                                             .UseBootstrap()
                                             .UseYamlFrontMatter()
                                             .UseGenericAttributes()
                                             .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                                             .Build();
            
            for (int i = 0; i < MDs.Count; i++)
            {
                StringBuilder nhtml = new StringBuilder();
                StringBuilder ohtml = new StringBuilder();

                foreach (NavItem item in navs.Items)
                {
                    ActiveState active = ActiveState.None;
                    string uid = Nav.GetNavItem(MDs[i]).UID;
                    string nid = Nav.SanitizeFilename(item.Title).Replace(" ", string.Empty);

                    if (item.Items.Any(t => t.UID == uid))
                    {
                        active = ActiveState.Child;
                        nhtml.AppendLine("<li class=\"panel expanded active\">" +
                                         $"<a class=\"area\" href=\"#{nid}\" data-parent=\"#main-nav\" data-toggle=\"collapse\">{item.Title}</a>");
                        nhtml.AppendLine($"<ul id=\"{nid}\" class=\"collapse in\">");
                    }
                    else
                    {
                        nhtml.AppendLine("<li class=\"panel collapsed\">" +
                                         $"<a class=\"area\" href=\"#{nid}\" data-parent=\"#main-nav\" data-toggle=\"collapse\">{item.Title}</a>");
                        nhtml.AppendLine($"<ul id=\"{nid}\" class=\"collapse\">");
                    }

                    ohtml.AppendLine($"<optgroup label=\"{item.Title}\">");

                    foreach (NavItem navItem in item.Items)
                    {

                        if (active == ActiveState.Child && uid == navItem.UID)
                        {
                            nhtml.AppendLine(ProcessNav(navItem, active, uid));
                            ohtml.AppendLine($"<option selected=\"selected\" value=\"{navItem.Link}\">{navItem.Title}</option>");
                        }
                        else
                        {
                            nhtml.AppendLine(ProcessNav(navItem, ActiveState.None, uid));
                            ohtml.AppendLine($"<option value=\"{navItem.Link}\">{navItem.Title}</option>");
                        }
                    }

                    nhtml.AppendLine("</ul></li></div>");
                    ohtml.AppendLine("</optgroup>");
                }


                navHtml = minifier.Minify(nhtml.ToString()).MinifiedContent;
                optHtml = "<select id=\"small-nav-dropdown\">" + minifier.Minify(ohtml.ToString()).MinifiedContent + "</select>";

                ProcessMD(MDs[i]);
            }//);

            //! Uncomment to use parallel processing. Useful when you have hundreds of files.
            // Parallel.For(0, MDs.Count, i => ProcessMD(MDs[i]));
        }
    }
}