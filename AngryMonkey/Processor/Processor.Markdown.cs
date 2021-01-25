using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AngryMonkey.Objects;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;
using WebMarkupMin.Core;

namespace AngryMonkey
{
    public partial class Processor
    {
        private HtmlMinifier minifier;

        private int counter = 0;

        public void ProcessPages()
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
                                        .UsePipeTables(new PipeTableOptions { RequireHeaderSeparator = true })
                                        .UseYamlFrontMatter()
                                        .UseGenericAttributes()
                                        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
                                        .Build();

            // Iterate through every page in the documentation (all hives)
            foreach ((string _, Page page) in pages)
            {
                // Acquire writeable filename from the Href
                string filename = $"{Destination}{page.Href.Replace("/", "\\")}";

                // Check if file has been modified, skip if not
                if (!Force)
                    if (!IsStale(page) && File.Exists(filename)) continue;

                string pageContents = page.Contents;

                // Replace all @ references with links
                ProcessLinks(ref pageContents);

                // Replace all ^ includes with fragment
                ProcessIncludes(ref pageContents);

                // Replace template elements and generate HTML from the markdown
                string result = templates[mainTemplate].Replace("{{BODY}}", Markdown.ToHtml(pageContents, pipeline))
                                                       .Replace("{{TITLE}}", page.Title)
                                                       .Replace("{{PARENT}}", page.Parent.Title)
                                                       .Replace("{{PARENT-HREF}}", page.Parent.Href)
                                                       .Replace("{{HIVE-HREF}}", page.Hive?.Path)
                                                       .Replace("{{HIVE}}", page.Hive?.Title);

                // Save the file
                File.WriteAllText(filename, minifier.Minify(result).MinifiedContent);

                // Add or update the hash
                if (hashes.ContainsKey(page.Filename))
                    hashes[page.Filename] = GetMD5(page);
                else
                    hashes.Add(page.Filename, GetMD5(page));

                counter++;
            }
        }

        private void ProcessLinks(ref string s)
        {
            foreach ((string key, Page page) in pages)
            {
                if (s.Contains($"@{key}"))
                    s = s.Replace($"@{key}", $"[{page.Title}]({page.Href})");
            }

        }

        private void ProcessIncludes(ref string s)
        {
            foreach ((string key, string value) in includes)
            {
                if (s.Contains($"^{key}"))
                    s = s.Replace($"^{key}", $"\n{value}\n");
            }
        }

        public Dictionary<string, string> GetFrontMatter(string contents)
        {
            if (!contents.StartsWith("---"))
            {
                return null;
            }

            Dictionary<string, string> yaml = new();
            string[] lines = contents.Split('\n');

            foreach (string line in lines.Skip(1))
            {
                if (line.Contains("---"))
                    break;

                string[] data = line.Split(':');

                yaml.Add(data[0].Trim(), data[1].Trim());
            }

            return yaml;
        }
        public string MonkeyMagic(string contents)
        {
            string[] lines = contents.Split('\n');

            StringBuilder sb = new();
            bool first = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("#### "))
                {
                    if (!first)
                    {
                        first = true;
                        sb.AppendLine("\n<div class=\"no example\">\n");
                    }
                    else
                    {
                        sb.AppendLine("\n</div><div class=\"no example\">\n");
                    }
                }

                if (first && line.StartsWith("## "))
                {
                    first = false;
                    sb.AppendLine("\n</div>\n");
                }

                sb.AppendLine(line);
            }

            if (first)
                sb.AppendLine("</div>");

            return sb.ToString();
        }
    }
}