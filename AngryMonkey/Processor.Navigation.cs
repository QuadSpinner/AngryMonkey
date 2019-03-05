using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AngryMonkey
{
    public partial class Processor
    {
        internal void GetIdentifiers(string dir)
        {
            IEnumerable<string> mds = Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories);

            foreach (string md in mds.Where(md => !md.ToLower().EndsWith(".params.md") && !md.Contains("--")))
            {
                identifiers.Add(GetNavItem(md));
            }
        }

        private static string ProcessNav(NavItem n)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<li class=\"sidenav-item\">");
            html.AppendLine(n.Items.Count > 0
                                ? "<a href=\"javascript:void(0)\" class=\"sidenav-link sidenav-toggle\">"
                                : $"<a href=\"{n.Link.Split('\\').Last().Replace(".md", ".html")}\" class=\"sidenav-link\">");

            html.AppendLine($"<div>{n.Title}</div>");
            html.AppendLine("</a>");

            if (n.Items.Count > 0)
            {
                html.AppendLine("<ul class=\"sidenav-menu\">");
                foreach (NavItem item in n.Items)
                {
                    html.AppendLine(ProcessNav(item));
                }

                html.AppendLine("</ul>");
            }

            html.AppendLine("</li>");

            return html.ToString();
        }

        private NavItem GetNavItem(string md)
        {
            string name = SanitizeFilename(md);

            var n = new NavItem(tt.ToTitleCase(name.Replace("-", " ")), md);

            string[] lines = File.ReadAllLines(md);

            for (int i = 1; i < 4; i++)
            {
                if (lines[i].Contains("uid:"))
                {
                    n.UID = "@" + lines[i].Split(':')[1].Trim();
                }

                if (lines[i].Contains("title:"))
                {
                    n.Title = lines[i].Split(':')[1].Trim();
                }

                if (lines[i].Contains("nav:"))
                {
                    n.Show = lines[i].Split(':')[1].Trim() == "true";
                }
            }

            n.Link = name + ".html";

            return n;
        }

        private static string SanitizeFilename(string md)
        {
            return Path.GetFileNameWithoutExtension(md).Contains("-") &&
                   int.TryParse(Path.GetFileNameWithoutExtension(md).Split('-')[0], out int _)
                       ? Path.GetFileNameWithoutExtension(md)
                             .Replace(Path.GetFileNameWithoutExtension(md).Split('-')[0] + "-", string.Empty)
                       : Path.GetFileNameWithoutExtension(md);
        }

        private NavItem ParseDirectory(string dir)
        {
            IEnumerable<string> dirs = Directory.EnumerateDirectories(dir);

            string dirName = tt.ToTitleCase(dir.Trim('\\').Split('\\').Last());

            if (dirName.Contains("-"))
            {
                dirName = dirName.Split('-')[1].Trim();
            }

            NavItem current = new NavItem(dirName);

            string[] mds = Directory.GetFiles(dir, "*.md");

            foreach (string md in mds.Where(md => !md.ToLower().EndsWith(".params.md") && !md.Contains("--")))
            {
                var temp = GetNavItem(md);
                if (temp.Show)
                    current.Items.Add(temp);
                MDs.Add(md);
            }

            foreach (string d in dirs)
            {
                current.Items.Add(ParseDirectory(d));
            }

            return current;
        }
    }
}