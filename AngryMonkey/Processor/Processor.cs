using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using AngryMonkey.Objects;
using Newtonsoft.Json;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor
    {
        private const string paramIdentifier = ".params.";
        private const string mainTemplate = "template";
        private const string skipPrefix = "_";
        private const string hiveFile = "hives.json";
        private const string varFile = "variables.json";

        private Dictionary<string, string> hashes = new();

        public Hive[] hives;

        public Dictionary<string, string> includes = new();

        public Dictionary<string, List<Link>> links = new();

        public Dictionary<string, Page> pages = new();

        public Dictionary<string, string> templates = new();
        public Dictionary<string, string> variables = new();

        public Processor()
        {
            minifier = new HtmlMinifier(new HtmlMinificationSettings(true)
            {
                MinifyInlineJsCode = true,
                WhitespaceMinificationMode = WhitespaceMinificationMode.Medium
            });

            Write("\n   AngryMonkey " + Assembly.GetExecutingAssembly().GetName().Version, true, ConsoleColor.DarkCyan);
        }

        public string Source { get; set; }

        public string Destination { get; set; }

        public bool Force { get; set; }

        public void Process()
        {
            hives = JsonConvert.DeserializeObject<Hive[]>(File.ReadAllText($"{Source}\\{hiveFile}"));
            variables =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"{Source}\\{varFile}"));

            LoadHashes();
            InitializeTemplates();
            Directory.CreateDirectory(Destination);

            ProcessHives();

            SaveHashes();
        }

        public void ProcessHives()
        {
            Write("", true);

            AddRootPages();

            foreach (Hive hive in hives)
            {
                // Preprocess XML changelogs
                if (hive.ProcessXmlChangelogs)
                {
                    Write("   Changelogs...");
                    PreprocessChangelogs($"{Source}\\{hive.Path}");
                    OK();
                }

                Write($"   {hive.Title}...");

                // Create hive data
                GetPages(hive);
                CreateNavigation(hive);
                OK();
            }

            Write("\n   Processing ");
            Write(pages.Count.ToString("000"), false, ConsoleColor.Yellow);
            Write(" pages...");
            ProcessPages();
            OK();

            if (counter > 0)
            {
                Write("   Creating search index...", false, ConsoleColor.DarkCyan);
                ProcessSearch();
                OK();
            }

            Write($"\n   {counter}", false, ConsoleColor.Cyan);
            Write(" pages updated.", true);

            Write("\n   The monkey is happy! Ooo ooo ooo aahh ahh!", true, ConsoleColor.Green);
            Write("\n", true);
        }

        private void AddRootPages()
        {
            foreach (string file in Directory.GetFiles(Source, "*.md", SearchOption.TopDirectoryOnly))
            {
                AddPage(null, file, new Link());
            }
        }

        private void ProcessSearch()
        {
            List<SearchObject> searches = new();

            foreach ((string _, Page page) in pages.Where(p => p.Value.Hive != null && !p.Value.Hive.ExcludeFromSearch))
            {
                searches.Add(page.ToSearchObject());
            }

            if (searches.Count != 0)
            {
                string jsonString = JsonConvert.SerializeObject(searches,
                                                                searches.GetType(),
                                                                Formatting.None,
                                                                new JsonSerializerSettings
                                                                {
                                                                    StringEscapeHandling =
                                                                        StringEscapeHandling.EscapeHtml
                                                                })
                                               .Replace("  ", "")
                                               .Replace("|", "")
                                               .Replace("`", "")
                                               .Replace("--", "")
                                               .Replace("\\u0022", "")
                                               .Replace("\\u0027", "'")

                                               //!STOP
                                               .Replace("\\n", " ")
                                               .Replace("\n", "");

                File.WriteAllText($"{Destination}\\search.json", jsonString);
            }
        }

        public void GetPages(Hive hive)
        {
            string path = $"{Source}\\{hive.Path}";

            // Get each folder which will become a navigation group
            string[] folders = Directory.GetDirectories(path, "*");

            links.Clear();

            if (File.Exists(path + "\\index.md"))
                AddPage(hive, path + "\\index.md", new Link());

            foreach (string folder in folders)
            {
                // Link references for all pages in this group
                List<Link> folderLinks = new();

                // Get a usable folder name string
                string folderName =
                    Strip(folder.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).Last());

                // Create link for the parent that all pages in this folder will share
                Link parent = new() { Href = folderName, Title = folderName };

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

        private Page AddPage(Hive hive, string file, Link parent)
        {
            string markdown = File.ReadAllText(file);

            if (Path.GetFileNameWithoutExtension(file).Contains(" "))
            {
                Write("\n   Space character found in ", false, ConsoleColor.Red);
                Write(Path.GetFileNameWithoutExtension(file), true, ConsoleColor.Yellow);
                Write("\n   End program. The monkey is ANGRY!\n\n", false, ConsoleColor.Red);
                Environment.Exit(0);
            }

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

            markdown = MonkeyMagic(markdown);

            // Construct the page
            Page page = new()
            {
                Hive = hive,
                Filename = file,
                Parent = parent,
                Contents = markdown,
                Title = yaml["title"],
                UID = yaml["uid"],
                Hidden = yaml.ContainsKey("show") && yaml["show"] == "no"
            };

            // If there is a secondary 'parameters' file, load and append it using the PARAMS template fragment
            if (hive != null && hive.ProcessProceduralFiles)
            {
                ProcessParameters(page);
                //string paramFile = page.Filename.Replace(".md", ".params.md");
                //if (File.Exists(paramFile))
                //{
                //    page.Contents += templates["params"].Replace("{{PARAMS}}", File.ReadAllText(paramFile));
                //}
            }

            pages.Add(page.UID, page);

            return page;
        }

        private void ProcessParameters(Page page)
        {
            string paramFile = Source + "\\_flubs\\" + page.UID + ".xml";

            if (File.Exists(paramFile))
            {
                XmlSerializer xs = new(typeof(List<Flub>));
                using var fs = new FileStream(paramFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                List<Flub> flubs = xs.Deserialize(fs) as List<Flub>;

                StringBuilder sb = new();

                sb.AppendLine("<h2 class=\"ui header dividing\">Properties</h2>");
                sb.AppendLine("<table class=\"ui table blue stackable basic properties-table\"><tbody>");

                foreach (Flub flub in flubs)
                {
                    if (flub.Description == "T")
                    {
                        sb.AppendLine($"<tr><td colspan='2' class='title'><h4 class=\"ui header\">{flub.Name}</h4></td><tr>");

                    }
                    else
                    {
                        if (flub.Flubs == null)
                        {
                            sb.AppendLine($"<tr><td>{flub.Name}</td><td>{flub.Description}</td><tr>");
                        }
                        else
                        {
                            sb.AppendLine($"<tr><td>{flub.Name}</td><td>{flub.Description} <br><br>");
                            foreach (Flub flubFlub in flub.Flubs)
                            {
                                sb.AppendLine($"<span class=\"choice\">{flubFlub.Name}</span>" +
                                              $"<span class=\"choice-description\">{flubFlub.Description ?? "Lorem ipsum dolor sit amet"}</span>");
                            }
                            sb.AppendLine("</td><tr>");
                        }
                    }

                }

                sb.AppendLine("</tbody>");
                sb.AppendLine("</table>");

                page.Contents += sb.ToString();
            }

        }

        public void CreateNavigation(Hive hive)
        {
            StringBuilder navHtml = new();

            foreach (Hive currentHive in hives)
            {
                navHtml.AppendLine($"<a class=\"item hive\" href=\"/{currentHive.Path}\">{currentHive.Title}</a>");

                if (hive.Path == currentHive.Path)
                {
                    // Iterate through every folder in this hive
                    foreach ((string key, List<Link> value) in links)
                    {
                        string title = key.Replace("-", " ");
                        navHtml.AppendLine("<div class=\"item\">");
                        navHtml.AppendLine($"<div class=\"header\">{title}</div>");
                        navHtml.AppendLine("<div class=\"menu\">");
                        // Iterate through every page within the folder
                        foreach (Link link in value)
                            navHtml.AppendLine($"<a class=\"item\" href=\"{link.Href}\">{link.Title}</a>");


                        navHtml.AppendLine("</div></div>");

                        /////////// 
                    }
                }
            }

            // Save the navigation fragment for Ajax loading later
            Directory.CreateDirectory($"{Destination}\\Navs\\");
            File.WriteAllText($"{Destination}\\Navs\\{hive.Path}-n.html",
                              minifier.Minify(navHtml.ToString()).MinifiedContent);
        }
    }
}