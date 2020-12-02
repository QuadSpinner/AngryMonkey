using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AngryMonkey.Objects;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.Tables;

namespace AngryMonkey
{
    public partial class Processor
    {
        // private string title = "";
        internal void ProcessMD(string md)
        {
            string title = "";
            string name = Nav.SanitizeFilename(md);
            string html = raw_html;

            //html = html.Replace("<!--REPLACE--OPT-->", optHtml);
            //html = html.Replace("<!--REPLACE--NAV-->", navHtml);
            html = html.Replace("<!--HIVE-->", BaseItem.UID);
            html = html.Replace("<!--REPLACE-SUPERNAV-->", MainNavHTML);

            #region Get Title

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

            //StringBuilder bread = new StringBuilder();

            //if (navs != null)
            //{
            //    bread.AppendLine("<ol class=\"breadcrumb visible-md visible-lg\">");
            //    bread.AppendLine(
            //        "<li class=\"breadcrumb-item\"><a href=\"/\">Home</a></li>");
            //    bread.AppendLine($"<li class=\"breadcrumb-item\"><a href=\"{BaseItem.Link}\">{BaseItem.Title}</a></li>");
            //    if (navs.Items.Any(i => i.Items.Any(x => x.Title == title)))
            //        bread.AppendLine(
            //            $"<li class=\"breadcrumb-item\">{navs.Items.First(i => i.Items.Any(x => x.Title == title))}</li>");
            //    bread.AppendLine($"<li class=\"breadcrumb-item active\">{title}</li>");
            //    bread.AppendLine("</ol>");
            //}

            #endregion Get Title

            StringBuilder raw = new StringBuilder();

            foreach (string s in lines.Skip(3))
            {
                if (s.StartsWith("^"))
                {
                    string include = $"{Nav.RootPath}\\source\\_includes\\{s.Replace("^", string.Empty)}.md";
                    if (File.Exists(include))
                        raw.AppendLine(File.ReadAllText(include));
                }
                else
                {
                    raw.AppendLine(ProcessLinks(s));
                }
            }

            if (!md.ToLower().Contains("index.md"))
            {
                string[] doc =
                    Regex.Replace(Regex.Replace(Markdown.ToPlainText(raw.ToString()), @"\{([^\}]+)\}", string.Empty),
                                  @"\<([^\>]+)\>",
                                  string.Empty)
                         .ToLower()
                         .Replace("\n", " ")
                         .Replace("'", string.Empty)
                         .Replace(" is ", " ")
                         .Replace(" that ", " ")
                         .Replace(" a ", " ")
                         .Replace("\\", " ")
                         .Replace("/", " ")
                         .Replace(",", string.Empty)
                         .Replace(":", string.Empty)
                         .Replace("!", string.Empty)
                         .Replace("`", string.Empty)
                         .Replace(".", " ")
                         .Replace("  ", " ")
                         .Replace(" an ", " ")
                         .Replace(" for ", " ")
                         .Replace(" this ", " ")
                         .Replace(" some ", " ")
                         .Replace(" other ", " ")
                         .Replace(" be ", " ")
                         .Replace(" to ", " ")
                         .Replace(" it ", " ")
                         .Replace(" from ", " ")
                         .Replace(" the ", " ")
                         .Replace("the ", " ")
                         .Replace(" can ", " ")
                         .Replace(" else ", " ")
                         .Replace(" you ", " ")
                         .Replace(" than ", " ")
                         .Replace(" of ", " ")

                         .Replace(" into ", " ")
                         .Replace(" lets ", " ")
                         .Replace(" let ", " ")
                         .Replace(" on ", " ")
                         .Replace(" even ", " ")
                         .Replace(" more ", " ")
                         .Replace(" help ", " ")
                         .Replace(" better ", " ")
                         .Replace(" gives ", " ")
                         .Replace(" easy ", " ")
                         .Replace(" go ", " ")
                         .Replace(" need ", " ")

                         .Replace(" are ", " ")
                         .Replace(" is ", " ")
                         .Replace(" choose ", " ")
                         .Replace(" see ", " ")
                         .Replace(" there ", " ")
                         .Replace(" give ", " ")
                         .Replace(" at ", " ")
                         .Replace(" here ", " ")
                         .Replace(" using ", " ")
                         .Replace(" and ", " ")
                         .Replace(" then ", " ")
                         .Replace(" found ", " ")
                         .Replace(" wont ", " ")
                         .Replace(" cant ", " ")
                         .Replace(" get ", " ")
                         .Replace(" will ", " ")
                         .Replace("-", " ")

                         .Replace(" we ", " ")
                         .Replace(" or ", " ")
                         .Replace(" only ", " ")
                         .Replace(" was ", " ")
                         .Replace(" also ", " ")
                         .Replace(" by ", " ")
                         .Replace(" has ", " ")
                         .Replace(" has ", " ")

                         .Replace(" your ", " ")
                         .Replace(" not ", " ")
                         .Replace(" have ", " ")
                         .Replace("All ", " ")
                         .Replace(" all ", " ")
                         .Replace(" these ", " ")
                         .Replace(" if ", " ")
                         .Replace("if ", " ")
                         .Replace(" their ", " ")
                         .Replace(" with ", " ")
                         .Replace(" as ", " ")

                         .Replace("  ", " ")
                         .Replace("  ", " ")
                         .Trim()
                         .Split(' ', StringSplitOptions.TrimEntries);

                StringBuilder temp = new StringBuilder();
                foreach (string s in doc.Distinct())
                {
                    temp.Append($"{s} ");
                }

                search.Add(new SearchObject
                {
                    url = Nav.GetNavItem(md).Link,
                    title = title,
                    hive = BaseItem.Title,
                    text = temp.ToString()
                });
            }

            StringBuilder output = new StringBuilder();
            output.AppendLine(Markdown.ToHtml(raw.ToString(), p));

            if (ProcessProceduralFiles)
            {
                string @params = $"{Path.GetDirectoryName(md)}\\{Path.GetFileNameWithoutExtension(md)}.params.md";

                //? PARAMETERS
                if (File.Exists(@params))
                {
                    output.AppendLine(ProcessParams(@params));
                    // AppendXML(@params, title);
                }
            }

            html = html.Replace("<!--REPLACE--BODY-->", $"<p class=\"faux-h1\">{title}</p>{output}");
            html = html.Replace("%%TITLE%%", title);

            try
            {
                string subpath = Path.GetDirectoryName(md.Replace(Nav.RootPath, Nav.RootPath + dst))
                                     .Replace("source\\", "");

                if (!subpath.EndsWith("\\"))
                    subpath += "\\";

                subpath = Nav.ReplaceNumbers(subpath);
                Directory.CreateDirectory(subpath);

                //File.WriteAllText(subpath + name + ".html", html);
                File.WriteAllText(subpath + name + ".html", minifier.Minify(html).MinifiedContent);
            }
            catch (IOException)
            {
                Console.WriteLine("ERROR: " + Nav.RootPath + dst + name + ".html could not be written.");
            }

            //Console.Write(".");
        }

        internal void ProcessRootMD(string md, bool createNav = true)
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
                if (s.StartsWith("^"))
                {
                    string include = $"{Nav.RootPath}\\source\\_includes\\{s.Replace("^", string.Empty)}.md";
                    if (File.Exists(include))
                        raw.AppendLine(File.ReadAllText(include));
                }
                else
                {
                    raw.AppendLine(ProcessLinks(s));
                }
            }

            StringBuilder output = new StringBuilder();

            //output.AppendLine("<div class=\"card\">");
            //output.AppendLine("<div class=\"card-body\">");
            output.AppendLine(Markdown.ToHtml(raw.ToString(), p));

            //output.AppendLine("</div></div>");
            html = html.Replace("%%TITLE%%", "Home");
            html = html.Replace("<!--REPLACE--BODY-->", output.ToString());
            html = html.Replace("<!--HIVE-->", createNav ? BaseItem.UID : "none");
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

            //if (s.Contains("@"))
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;
            //    Console.WriteLine($"\n@@@@ {title}");
            //    Console.ForegroundColor = ConsoleColor.Gray;
            //}

            return s;
        }

        private string ProcessParams(string s)
        {
            List<StringBuilder> sbs = new List<StringBuilder>();
            string[] lines = File.ReadAllLines(s);
            //List<string> titles = new List<string>();

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
                    string include = $"{Nav.RootPath}\\source\\_includes\\{line.Replace("^", string.Empty)}.md";
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

            string mdd = sbs.Last().ToString().Trim('{').Trim('}');

            html.AppendLine("<br><h3>Node Properties</h3>");
            html.AppendLine("<div class=\"params\">");
            html.AppendLine(Markdown.ToHtml(mdd, p).Replace("class=\"table\"", "class=\"table table-borderless\""));
            html.AppendLine("</div>");

            return html.ToString();
        }
    }
}