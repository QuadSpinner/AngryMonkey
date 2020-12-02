using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using AngryMonkey.Objects;
using Newtonsoft.Json;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor2
    {
        public Dictionary<string, string> includes = new Dictionary<string, string>();

        public Dictionary<string, List<Link>> links = new Dictionary<string, List<Link>>();

        public Dictionary<string, Page> pages = new Dictionary<string, Page>();

        public Dictionary<string, string> templates = new Dictionary<string, string>();
        private Dictionary<string, string> hashes = new Dictionary<string, string>();

        public Hive2[] hives;

        private const string paramIdentifier = ".params.";
        private const string mainTemplate = "template";
        private const string skipPrefix = "_";

        public string Source { get; set; }

        public string Destination { get; set; }

        public bool Force { get; set; }


        public Processor2()
        {
            minifier = new HtmlMinifier(new HtmlMinificationSettings(true)
            {
                MinifyInlineJsCode = true,
                WhitespaceMinificationMode = WhitespaceMinificationMode.Medium
            });

        }

        public void Process()
        {
            Write("", true);

            hives = JsonConvert.DeserializeObject<Hive2[]>(File.ReadAllText(Source + "\\hives.json"));

            LoadHashes();
            InitializeTemplates();
            Directory.CreateDirectory(Destination);
            Write("", true);
            ProcessHives();

            SaveHashes();
        }

        public void InitializeTemplates()
        {
            Write("   Loading templates...");

            // Load templates into Dictionaries
            string[] files = Directory.GetFiles(Source + "\\_template", "*.html");
            foreach (string file in files)
            {
                templates.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
            }

            files = Directory.GetFiles(Source + "\\_includes", "*.md");
            foreach (string file in files)
            {
                includes.Add(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file));
            }

            StringBuilder navHtml = new StringBuilder();
            foreach (Hive2 hive in hives)
            {
                navHtml.AppendLine($"<li><a class=\"top-nav\" href=\"/{hive.Path}/\">{hive.Title}</a></li>");
            }

            templates[mainTemplate] = templates[mainTemplate].Replace("{{TOPNAV}}", navHtml.ToString());

            // Get hashes for the templates and fragments
            string md5templates = CreateMd5ForFolder(Source + "\\_template");
            string md5includes = CreateMd5ForFolder(Source + "\\_includes");

            // If the hashes don't match, force rebuild of all pages

            if (hashes.ContainsKey("_template"))
            {
                if (md5templates != hashes["_template"])
                    Force = true;

                hashes["_template"] = md5templates;
            }
            else
            { hashes.Add("_template", md5templates); }

            if (hashes.ContainsKey("_includes"))
            {
                if (md5includes != hashes["_includes"])
                    Force = true;

                hashes["_includes"] = md5includes;
            }
            else
            { hashes.Add("_includes", md5includes); }

            OK();
        }

        public void ProcessHives()
        {
            foreach (Hive2 hive in hives)
            {
                Write($"   {hive.Title}...");

                // Create hive data
                GetPages(hive);
                CreateNavigation(hive);
                OK();
            }
            Write("", true);
            Write("   Processing ");
            Write(pages.Count.ToString("000"), false, ConsoleColor.Yellow);
            Write(" pages...");
            ProcessPages();
            OK();
            Write("", true);
            Write($"   {(counter)}", false, ConsoleColor.Cyan);
            Write(" pages updated.", true);
            Write("\n", true);

            Write("   The monkey is happy! Ooo ooo ooo aahh ahh!", true, ConsoleColor.Green);
            Write("\n", true);
        }

        public void GetPages(Hive2 hive)
        {
            string path = $"{Source}\\{hive.Path}";

            // Get each folder which will become a navigation group
            string[] folders = Directory.GetDirectories(path, "*");

            links.Clear();

            AddPage(hive, path + "\\index.md", new Link());

            foreach (string folder in folders)
            {
                // Link references for all pages in this group
                List<Link> folderLinks = new List<Link>();

                // Get a usable folder name string
                string folderName = Strip(folder.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).Last());

                // Create link for the parent that all pages in this folder will share
                Link parent = new Link { Href = folderName, Title = folderName };

                string[] files = Directory.GetFiles(folder, "*.md");

                Directory.CreateDirectory($"{Destination}\\{hive.Path}\\{folderName}\\");
                foreach (string file in files)
                {
                    // Skip procedural or force-skip files
                    if (file.Contains(paramIdentifier) || file.StartsWith(skipPrefix))
                        continue;

                    Page page = AddPage(hive, file, parent);

                    if (!page.Hidden)
                        folderLinks.Add(new Link { Href = page.Href, Title = page.Title });
                }

                links.Add(folderName, folderLinks);
            }
        }

        private Page AddPage(Hive2 hive, string file, Link parent)
        {
            string markdown = File.ReadAllText(file);

            // Get YAML options
            Dictionary<string, string> yaml = GetFrontMatter(markdown);

            // Do not allow duplicates!
            if (pages.ContainsKey(yaml["uid"]))
            {
                Write("\n   Duplicate UID found ", false, ConsoleColor.Red);
                Write(yaml["uid"], true, ConsoleColor.Yellow);
                Write("\n   End program. The monkey is ANGRY!\n\n", false, ConsoleColor.Red);
                Environment.Exit(0);
            }

            // Construct the page
            Page page = new Page
            {
                Hive = hive,
                Filename = file,
                Parent = parent,
                Contents = markdown,
                Title = yaml["title"],
                UID = yaml["uid"],
                Hidden = yaml.ContainsKey("show") && yaml["show"] == "no" || file.ToLower().Contains("index.html")
            };


            // If there is a secondary 'parameters' file, load and append it using the PARAMS template fragment
            if (hive.ProcessProceduralFiles)
            {
                string paramFile = page.Filename.Replace(".md", ".params.md");
                if (File.Exists(paramFile))
                {
                    page.Contents += templates["params"].Replace("{{PARAMS}}", File.ReadAllText(paramFile));
                }
            }

            pages.Add(page.UID, page);

            return page;
        }

        public void CreateNavigation(Hive2 hive)
        {
            StringBuilder navHtml = new StringBuilder();

            // Iterate through every folder in this hive
            foreach ((string key, List<Link> value) in links)
            {
                navHtml.AppendLine("<li class=\"panel collapsed\">");
                navHtml.AppendLine($"<a class=\"area\" href=\"#{key}\" data-toggle=\"collapse\">{key}</a>");
                navHtml.AppendLine($"<ul id=\"{key}\" class=\"collapse\">");

                // Iterate through every page within the folder
                foreach (Link link in value)
                    navHtml.AppendLine($"<li class=\"xref\"><a href=\"{link.Href}\">{link.Title}</a></li>");

                navHtml.AppendLine("</ul>");
                navHtml.AppendLine("</li>");
            }

            // Save the navigation fragment for Ajax loading later
            Directory.CreateDirectory($"{Destination}\\Navs\\");
            File.WriteAllText($"{Destination}\\Navs\\{hive.Path}-n.html", minifier.Minify(navHtml.ToString()).MinifiedContent);
        }
    }
}