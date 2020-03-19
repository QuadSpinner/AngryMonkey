using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;

namespace AngryMonkey
{
    public partial class Processor
    {
        internal void ProcessMD(string md)
        {
            string name = Nav.SanitizeFilename(md);
            string html = raw_html;

            html = html.Replace("<!--REPLACE--NAV-->", navHtml);
            html = html.Replace("<!--REPLACE-SUPERNAV-->", MainNavHTML);

            #region Get Title

            string title = "";
            string[] lines = File.ReadAllLines(md);

            if (lines.Length > 3)
            {
                for (int i = 1; i < 3; i++)
                {
                    if (lines[i].Contains("title:"))
                    {
                        title = lines[i].Split(':')[1].Trim();
                    }
                }
            }

            if (string.IsNullOrEmpty(title.Trim()))
                title = name.Replace("-", " ");

            StringBuilder bread = new StringBuilder();

            bread.AppendLine("<ol class=\"breadcrumb\">");
            bread.AppendLine(
                "<li class=\"breadcrumb-item\"><a href=\"/index.html\">Home</a></li>");
            bread.AppendLine($"<li class=\"breadcrumb-item\"><a href=\"{BaseItem.Link}\">{BaseItem.Title}</a></li>");
            if (navs.Items.Any(i => i.Items.Any(x => x.Title == title)))
                bread.AppendLine(
                    $"<li class=\"breadcrumb-item\">{navs.Items.First(i => i.Items.Any(x => x.Title == title))}</li>");
            bread.AppendLine($"<li class=\"breadcrumb-item active\">{title}</li>");
            bread.AppendLine("</ol>");

            #endregion Get Title

            StringBuilder raw = new StringBuilder();

            foreach (string s in lines)
            {
                //bool f = s.Contains("@installing");
                //if (f)
                //{
                //    f = f;
                //}
                raw.AppendLine(ProcessLinks(s));
            }

            StringBuilder output = new StringBuilder();

            //output.AppendLine("<div class=\"card\">");
            //output.AppendLine("<div class=\"card-header\"><!--REPLACE--BREADCRUMBS--></div>");
            //output.AppendLine("<div class=\"card-body\">");
            output.AppendLine(Markdown.ToHtml(raw.ToString(), p));
            //output.AppendLine("</div></div>");

            if (ProcessProceduralFiles)
            {
                string @params = $"{Path.GetDirectoryName(md)}\\{Path.GetFileNameWithoutExtension(md)}.params.md";

                //? PARAMETERS
                if (File.Exists(@params))
                {
                    output.AppendLine(ProcessParams(@params));
                }
            }

            if (ProcessExampleFiles)
            {
                //! EXAMPLES
                output.AppendLine(ProcessExamples(md));
            }

            if (ProcessTutorialFiles)
            {
                //! TUTORIALS
                output.AppendLine(ProcessTutorials(md));
            }

            html = html.Replace("<!--REPLACE--BODY-->", $"{bread}<br>{output}");
            html = html.Replace("%%TITLE%%", title);
            //html = html.Replace("<!--REPLACE--BREADCRUMBS-->", bread.ToString());

            try
            {
                string subpath = Path.GetDirectoryName(md.Replace(Nav.RootPath, Nav.RootPath + dst))
                                     .Replace("source\\", "");
                if (!subpath.EndsWith("\\"))
                    subpath += "\\";

                subpath = Nav.ReplaceNumbers(subpath);

                Directory.CreateDirectory(subpath);

                File.WriteAllText(subpath + name + ".html", html);
                // File.WriteAllText(subpath + name + ".html", minifier.Minify(html).MinifiedContent);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR: " + Nav.RootPath + dst + name + ".html could not be written.");
            }

            //Console.Write(".");
        }

        internal void ProcessRootMD(string md)
        {
            p = new MarkdownPipelineBuilder().UsePipeTables(new PipeTableOptions { RequireHeaderSeparator = true })
                                             .UseBootstrap()
                                             .UseYamlFrontMatter()
                                             .UseGenericAttributes()
                                             .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                                             .Build();

            string name = Nav.SanitizeFilename(md);
            string html = raw_html;

            html = html.Replace("<!--REPLACE-SUPERNAV-->", MainNavHTML);

            string[] lines = File.ReadAllLines(md);

            StringBuilder raw = new StringBuilder();

            foreach (string s in lines)
            {
                raw.AppendLine(ProcessLinks(s));
            }

            StringBuilder output = new StringBuilder();

            //output.AppendLine("<div class=\"card\">");
            //output.AppendLine("<div class=\"card-body\">");
            output.AppendLine(Markdown.ToHtml(raw.ToString(), p));
            //output.AppendLine("</div></div>");
            html = html.Replace("%%TITLE%%", "Home");
            html = html.Replace("<!--REPLACE--BODY-->", output.ToString());
            try
            {
                File.WriteAllText(Nav.RootPath + dst + name + ".html", minifier.Minify(html).MinifiedContent);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR: " + Nav.RootPath + dst + name + ".html could not be written.");
            }

            //Console.Write(".");
        }

        private static string ProcessLinks(string s)
        {
            if (s.Contains("@"))
            {
                s = Nav.identifiers.Where(i => i.UID != null)
                       .Aggregate(s,
                                  (current, identifier)
                                      => current.Replace(identifier.UID,
                                                         $"[{identifier.Title}]({identifier.Link})"));
            }

            return s;
        }

        private string ProcessParams(string s)
        {
            List<StringBuilder> sbs = new List<StringBuilder>();
            string[] lines = File.ReadAllLines(s);
            List<string> titles = new List<string>();

            StringBuilder TabStrip = new StringBuilder();

            TabStrip.AppendLine(
                "<br><h6 class=\"ml-2\">Node Properties</h6><div class=\"nav-tabs-top mb-4\"><ul class=\"nav nav-sm nav-tabs\">");
            bool first = false;
            foreach (string title in lines.Where(l => l.StartsWith("# ["))
                                          .Select(line => line.Split(']')[0].Split('[')[1]))
            {
                if (!first)
                {
                    TabStrip.AppendLine(
                        $"<li class=\"nav-item\"><a class=\"nav-link active\" data-toggle=\"tab\" href=\"#params-{title.Replace(" ", "-")}\">{title}</a></li>");
                    first = true;
                }
                else
                {
                    TabStrip.AppendLine(
                        $"<li class=\"nav-item\"><a class=\"nav-link\" data-toggle=\"tab\" href=\"#params-{title.Replace(" ", "-")}\">{title}</a></li>");
                }

                titles.Add(title);
            }

            TabStrip.AppendLine("</ul>");

            StringBuilder current = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.StartsWith("# ["))
                {
                    if (current.ToString().Length > 10)
                        sbs.Add(current);
                    current = new StringBuilder();
                    continue;
                }

                if (line.StartsWith("^"))
                {
                    string include = $"{Nav.RootPath}Includes\\{line.Replace("^", string.Empty)}.md";
                    if (File.Exists(include))
                        current.AppendLine(File.ReadAllText(include));
                }
                else
                {
                    current.AppendLine(line);
                }
            }

            sbs.Add(current);

            StringBuilder html = new StringBuilder();

            if (titles.Count == 0)
            {
                string mdd = sbs.Last().ToString().Trim('{').Trim('}');

                html.AppendLine("<br><h6 class=\"ml-2\">Node Properties</h6>");
                html.AppendLine("<div class=\"card\"><div class=\"card-body params\">");
                html.AppendLine(Markdown.ToHtml(mdd, p)
                                        .Replace("class=\"table\"", "class=\"table table-borderless\""));
                html.AppendLine("</div></div>");
            }
            else
            {
                html.AppendLine(TabStrip.ToString());

                html.AppendLine("<div class=\"tab-content\">");

                int titleIndex = 0;
                foreach (StringBuilder sb in sbs)
                {
                    if (sb.Length > 1)
                    {
                        string title = titles[titleIndex];
                        html.AppendLine(
                            titleIndex == 0
                                ? $"<div class=\"tab-pane params fade active show\" id=\"params-{title.Replace(" ", "-")}\"><div class=\"card-body\">"
                                : $"<div class=\"tab-pane params fade show\" id=\"params-{title.Replace(" ", "-")}\"><div class=\"card-body\">");

                        string mdd = sb.ToString().Trim('{').Trim('}');
                        html.AppendLine(Markdown.ToHtml(mdd, p)
                                                .Replace("class=\"table\"", "class=\"table table-borderless\""));
                        html.AppendLine("</div></div>");
                        titleIndex++;
                    }
                }

                html.AppendLine("</div></div>"); //! Close TAB-CONTENT / NAV
            }

            return html.ToString();
        }

        private string ProcessExamples(string file)
        {
            string[] dirs = Directory.GetDirectories(Nav.RootPath + ScreenshotPath);

            // Get the node id
            string id = Path.GetFileNameWithoutExtension(file);

            // For each directory matching our node name
            IEnumerable<string> enumerable = dirs.Where(s => s.Contains(id + "--"));

            if (!enumerable.Any())
                return "";

            List<string> titles = new List<string>();
            StringBuilder TabStrip = new StringBuilder();

            TabStrip.AppendLine(
                "<br><h6 class=\"ml-2\">Examples</h6><div class=\"examples\">" +
                "<div class=\"nav-tabs-left mb-4\"><ul class=\"nav nav-sm nav-tabs\">");
            bool first = false;

            foreach (string title in enumerable.Select(
                x => x.Split(new[] { "--" }, StringSplitOptions.RemoveEmptyEntries).Last().Replace("-", " ")))
            {
                if (!first)
                {
                    TabStrip.AppendLine(
                        $"<li class=\"nav-item\"><a class=\"nav-link active\" data-toggle=\"tab\" href=\"#ex-{title.Replace(" ", "-")}\">{title.Replace("-", " ")}</a></li>");
                    first = true;
                }
                else
                {
                    TabStrip.AppendLine(
                        $"<li class=\"nav-item\"><a class=\"nav-link\" data-toggle=\"tab\" href=\"#ex-{title.Replace(" ", "-")}\">{title.Replace("-", " ")}</a></li>");
                }

                titles.Add(title);
            }

            TabStrip.AppendLine("</ul>");

            StringBuilder content = new StringBuilder();

            content.AppendLine(TabStrip.ToString());
            content.AppendLine("<div class=\"tab-content\">");

            int titleIndex = 0;
            foreach (string dir in enumerable)
            {
                // Individual example content
                StringBuilder sb = new StringBuilder();

                // Get all images
                string[] images = Directory.GetFiles(dir, "*.png");
                string[] texts = Directory.GetFiles(dir, "*.txt");

                // Create markup

                sb.AppendLine(images.Length > 3 ? "<div class=\"card-columns\">" : "<div class=\"card-deck\">");

                for (int index = 0; index < images.Length; index++)
                {
                    string image = images[index];
                    string text = texts[index];
                    sb.AppendLine("<div class=\"card\">");
                    sb.AppendLine(
                        $"<img   class=\"card-img-top zoom\" src=\"/images/ref/{Path.GetFileName(image)}\" />");
                    sb.AppendLine("<div class=\"card-footer\"><ul>");
                    foreach (string[] split in File.ReadAllLines(text).Select(line => line.Split('=')))
                    {
                        sb.AppendLine("<li>" + split[0].Trim() + " <code>" + split[1] + "</code></li>");
                    }

                    sb.AppendLine("</ul></div>");
                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>");

                // Add to main content

                string title = titles[titleIndex];
                content.AppendLine(
                    titleIndex == 0
                        ? $"<div class=\"tab-pane params fade active show\" id=\"ex-{title.Replace(" ", "-")}\"><div class=\"card-body\">"
                        : $"<div class=\"tab-pane params fade show\" id=\"ex-{title.Replace(" ", "-")}\"><div class=\"card-body\">");

                content.AppendLine(sb.ToString());

                content.AppendLine("</div></div>");
                titleIndex++;
            }

            content.AppendLine("</div>");
            content.AppendLine("</div>");
            content.AppendLine("</div>");

            return content.ToString();
        }

        private string ProcessTutorials(string file)
        {
            string id = Path.GetFileNameWithoutExtension(file);

            if (!Directory.Exists(Nav.RootPath + TutorialsPath + id))
                return "";

            string dir = Nav.RootPath + TutorialsPath + id;

            // Individual example content
            StringBuilder sb = new StringBuilder();

            // Get all images
            string[] images = Directory.GetFiles(dir, "*.png");
            string[] texts = Directory.GetFiles(dir, "*.txt");

            texts = texts.Where(s => !s.Contains("--Description")).ToArray();

            // Create markup

            sb.AppendLine("<div class=\"tutorial\">");
            for (int index = 0; index < images.Length; index++)
            {
                string image = images[index];
                string text = texts[index];
                sb.AppendLine("<div class=\"card\">");
                sb.AppendLine(
                    $"<img class=\"card-img-top\" src=\"/images/tutorials/{id}/{Path.GetFileName(image)}\" />");
                sb.AppendLine("<div class=\"card-footer\">");
                sb.AppendLine(
                    $"<h4>{index + 1}. {Path.GetFileNameWithoutExtension(image).Split(new[] { "--" }, StringSplitOptions.RemoveEmptyEntries).Last()}</h4>");

                sb.AppendLine("<ul class=\"checklist\">");

                string[] data = File.ReadAllLines(text);

                if (data == null || data.Length == 0)
                {
                    sb.AppendLine("<li>Leave settings at default.</li>");
                }
                else
                {
                    foreach (string[] split in data.Select(line => line.Split('=')))
                    {
                        sb.AppendLine("<li><span class=\"property\">" + split[0].Trim() + "</span> <code>" + split[1] +
                                      "</code></li>");
                    }
                }

                sb.AppendLine("</ul>");

                if (File.Exists(dir + "\\" + Path.GetFileNameWithoutExtension(image) + "--Description.txt"))
                    sb.AppendLine(Markdown.ToHtml(
                                      File.ReadAllText(dir + "\\" + Path.GetFileNameWithoutExtension(image) +
                                                       "--Description.txt"),
                                      p));
                else
                    sb.AppendLine("<p>CAPTION GOES HERE</p>");
                sb.AppendLine("</div>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div>");

            return sb.ToString();
        }

        public void ProcessChangelogs(string json = "Z:\\Git\\Gaea\\Gaea-Docs\\changelogs.json")
        {
            Changelog[] logs = Changelog.FromJson(File.ReadAllText(json));

            StringBuilder all = new StringBuilder();

            string card_template = File.ReadAllText(Nav.RootPath + CardTemplate);

            foreach (Changelog log in logs)
            {
                string card = card_template.Replace("<!--HEADER-->",
                                                    $"Gaea {log.Version}<br><small class=\"text-muted\">{Convert.ToDateTime(log.PubDate):yyyy MMMM dd}</small>");

                if (log.Url == null || string.IsNullOrEmpty(log.Url.ToString()))
                {
                    card = card.Replace("<!--FOOTER-->", "This version has been archived.");
                }
                else
                {
                    card = card.Replace("<!--FOOTER-->",
                                        $"<a href=\"{log.Url}\" class=\"btn btn-sm btn-primary\">Download {log.Version}</a>");
                }

                StringBuilder cl = new StringBuilder();
                cl.AppendLine("<ul class=\"changelog\">");
                foreach (string[] split in log.Notes.Select(note => note.Split(']')))
                {
                    string type = split[0].Replace("[", "");
                    cl.AppendLine($"<li class=\"{type.ToLower()}\"><span>{type}</span> {split[1]}</li>");
                }

                cl.AppendLine("</ul>");

                all.AppendLine(card.Replace("<!--BODY-->", cl.ToString()));
            }

            string html = raw_html;

            html = html.Replace("<!--REPLACE--NAV-->", navHtml);

            html = html.Replace("<!--REPLACE--BODY-->", all.ToString());

            File.WriteAllText(Nav.RootPath + dst + "changelogs.html", minifier.Minify(html).MinifiedContent);
        }
    }
}