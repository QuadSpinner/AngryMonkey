﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AngryMonkey
{
    public partial class Processor
    {
        private NavItem ParseDirectory(string dir)
        {
            IEnumerable<string> dirs = Directory.EnumerateDirectories(dir);

            string dirName = Nav.tt.ToTitleCase(dir.Trim('\\').Split('\\').Last());

            if (dirName.Contains("-"))
            {
                dirName = dirName.Split('-')[1].Trim();
            }

            NavItem current = new NavItem(dirName);

            string[] mds = Directory.GetFiles(dir, "*.md");

            foreach (string md in mds.Where(md => !md.ToLower().EndsWith(".params.md") && !md.Contains("--")))
            {
                var temp = Nav.GetNavItem(md);
                temp.Link = Nav.ReplaceNumbers(temp.Link.Replace(Nav.RootPath + "source", string.Empty).Replace("\\", "/").Replace(".md", ".html"));

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

        private static string ProcessNav(NavItem n)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<li class=\"sidenav-item\">");

            if (n.Items.Count > 0)
            {
                html.AppendLine("<a href=\"javascript:void(0)\" class=\"sidenav-link sidenav-toggle\">");
            }
            else
            {
                string link = n.Link.Replace(Nav.RootPath, string.Empty).Replace("\\", "/").Replace(".md", ".html");
                html.AppendLine($"<a href=\"{link}\" class=\"sidenav-link\">");
            }

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
    }
}