using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor
    {
        public const string BodyTemplate = @"_template\index.1.html";

        internal NavItem BaseItem = new NavItem("User Guide", "guide.html") { UID = "guide" };

        public string CardTemplate = @"_template\card.frag";

        private List<string> MDs = new List<string>();

        private HtmlMinifier minifier = new HtmlMinifier(new HtmlMinificationSettings(true)
        {
            MinifyInlineJsCode = true,
            WhitespaceMinificationMode = WhitespaceMinificationMode.Medium
        });

        private string navHtml = "";
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
                nhtml.AppendLine("<div id=\"layout-sidenav\" class=\"layout-sidenav sidenav sidenav-vertical bg-sidenav-theme\">" +
                                 $"<ul class=\"sidenav-inner py-1 {BaseItem.UID}\">");

                foreach (NavItem item in navs.Items)
                {
                    ActiveState active = ActiveState.None;
                    string uid = Nav.GetNavItem(MDs[i]).UID;
               
         
                    if (item.Items.Any() && item.Items.Any(t => t.UID == uid) 
                        || item.Items.Any() && item.Items.Any(t => t.Items.Any() && t.Items.Any(x => x.Items.Any(y => y.UID == uid))))
                    {
                        active = ActiveState.Child;
                    }
                    nhtml.AppendLine(ProcessNav(item, active, uid));
                }

                nhtml.AppendLine("</ul></div>");


                navHtml = minifier.Minify(nhtml.ToString()).MinifiedContent;

                ProcessMD(MDs[i]);
            }

            //! Uncomment to use parallel processing. Useful when you have hundreds of files.
            // Parallel.For(0, MDs.Count, i => ProcessMD(MDs[i]));
        }
    }
}