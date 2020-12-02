using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AngryMonkey.Objects;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor
    {
        public string BodyTemplate = @"\source\_template\index.2.html";
        public string CardTemplate = @"\source\_template\card.frag";

        internal NavItem BaseItem = new NavItem("User Guide", "guide.html") { UID = "guide" };


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

        internal List<SearchObject> search = new List<SearchObject>();

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

            CreateNavigation();

            for (int i = 0; i < MDs.Count; i++)
            {
                ProcessMD(MDs[i]);
            }

            //! Uncomment to use parallel processing. Useful when you have hundreds of files.
            //Parallel.For(0, MDs.Count, i => ProcessMD(MDs[i]));
        }
    }
}