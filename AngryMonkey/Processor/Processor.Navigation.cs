using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Gaea.Internals.Online;

namespace AngryMonkey
{
    public partial class Processor
    {
        private NavItem ParseDirectory(string dir)
        {
            IEnumerable<string> dirs = Directory.EnumerateDirectories(dir);

            string dirName = dir.Contains("Bleeding") ? dir.Trim('\\').Split('\\').Last()
                                 : Nav.tt.ToTitleCase(dir.Trim('\\').Split('\\').Last());

            if (dirName.Contains("-"))
            {
                dirName = dirName.Split('-')[1].Trim();
            }

            NavItem current = new NavItem(dirName);

            string[] xml = Directory.GetFiles(dir, "*.update");

            XmlSerializer xs = new XmlSerializer(typeof(UpdateManifest));

            foreach (string x in xml)
            {
                UpdateManifest manifest;
                using (var fs = new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    manifest = xs.Deserialize(fs) as UpdateManifest;
                }

                string version = Version.Parse(manifest.Version).ToString(x.Contains("Bleeding") || x.Contains("Preview") ? 4 : 3);
                string versionSafe = version.Replace(".", "_");

                StringBuilder md = new StringBuilder();

                md.AppendLine("---");
                md.AppendLine("uid: gaea_" + versionSafe);
                md.AppendLine("title: Gaea " + version);
                md.AppendLine("---\n\n");
                md.AppendLine($"**Released on {manifest.ReleaseDate:dd MMMM yyyy}**\n");

                md.AppendLine($"[Download Full Installer]({manifest.URL})\n");
                if (manifest.PatchSize > 0)
                    md.AppendLine($"[Download Patch]({manifest.PatchURL})\n");
                md.AppendLine("</div></div>");

                md.AppendLine("<br><h6 class=\"ml-2\">Release Notes</h6>");


                md.AppendLine("<div class=\"card\">");
                md.AppendLine("<div class=\"card-body release-note\">\n");
                md.AppendLine(manifest.FullDescription);
                md.AppendLine("\n</div></div>");
                
                File.WriteAllText($"{Path.GetDirectoryName(x)}\\{version}.md", md.ToString());
            }

            string[] mds = Directory.GetFiles(dir, "*.md");

            if (dir.Contains("changelogs"))
                mds = mds.OrderByDescending(x => x).ToArray();

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

        private static string ProcessNav(NavItem n, ActiveState active, string uid)
        {
            StringBuilder html = new StringBuilder();

            if (n.UID == uid)
                active = ActiveState.Self;

            string activeClass = active == ActiveState.Self ? "active" : "";
            string openClass = active == ActiveState.Child ? "open" : "";
            html.AppendLine($"<li class=\"sidenav-item {openClass}\">");
            if (n.Items.Count > 0)
            {
                html.AppendLine("<a href=\"javascript:void(0)\" class=\"sidenav-link sidenav-toggle\">");
            }
            else
            {
                string link = n.Link.Replace(Nav.RootPath, string.Empty).Replace("\\", "/").Replace(".md", ".html");
                html.AppendLine($"<a href=\"{link}\" class=\"sidenav-link {activeClass}\">");
            }

            html.AppendLine($"<div>{n.Title}</div>");
            html.AppendLine("</a>");

            if (n.Items.Count > 0)
            {
                html.AppendLine("<ul class=\"sidenav-menu\">");
                foreach (NavItem item in n.Items)
                {
                    html.AppendLine(ProcessNav(item, active, uid));
                }

                html.AppendLine("</ul>");
            }

            html.AppendLine("</li>");

            return html.ToString();
        }
    }


}