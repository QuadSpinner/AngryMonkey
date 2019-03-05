using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor
    {
        public const string BodyTemplate = @"_template\index.1.html";

        internal NavItem BaseItem = new NavItem("User Guide", "guide.html") {UID = "guide"};

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
            RootPath = rootPath;
            raw_html = File.ReadAllText(RootPath + BodyTemplate);
        }

        public string RootPath { get; set; }

        public string ScreenshotPath { get; set; } = @"Z:\Sync\QuadSpinner DELTA\Help\Reference";

        public bool ProcessProceduralFiles { get; set; } = true;

        public bool ProcessExampleFiles { get; set; } = true;

        internal string src { get; set; } = @"source\";

        internal string dst { get; set; } = @"docs\";


        internal void Process()
        {
            StringBuilder nhtml = new StringBuilder();
            navs = ParseDirectory(RootPath + src);
            p = new MarkdownPipelineBuilder().UsePipeTables(new PipeTableOptions {RequireHeaderSeparator = true})
                                             .UseBootstrap()
                                             .UseYamlFrontMatter()
                                             .UseGenericAttributes()
                                             .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                                             .Build();

            nhtml.AppendLine(
                $"<div id=\"layout-sidenav\"\r\n class=\"layout-sidenav-horizontal sidenav sidenav-horizontal bg-sidenav-theme container-p-x flex-grow-0\">\r\n <div class=\"container d-flex\">\r\n <ul class=\"sidenav-inner {BaseItem.UID}\">");
            foreach (NavItem item in navs.Items)
            {
                nhtml.AppendLine(ProcessNav(item));
            }

            nhtml.AppendLine("</ul></div></div>");

            navHtml = minifier.Minify(nhtml.ToString()).MinifiedContent;

            Parallel.For(0, MDs.Count, i => ProcessMD(MDs[i]));
        }


    }
}