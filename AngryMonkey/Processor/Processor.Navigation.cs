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

                string version = Version.Parse(manifest.Version).ToString(4);
                string versionSafe = version.Replace(".", "_");

                StringBuilder md = new StringBuilder();

                md.AppendLine("---");
                md.AppendLine("uid: gaea_" + versionSafe);
                md.AppendLine("title: Gaea " + version);
                md.AppendLine("---\n\n");
                md.AppendLine($"**Released on {manifest.ReleaseDate:dd MMMM yyyy}**\n");
                md.AppendLine($"<a href=\"{manifest.URL}\">Download {manifest.Size / 1024.0 / 1024.0:F}MB</a> <br>");
                md.AppendLine("\n");
                md.AppendLine("<div class=\"release-note\">\n");
                md.AppendLine(manifest.FullDescription);
                md.AppendLine("</div>");

                File.WriteAllText($"{Path.GetDirectoryName(x)}\\{version}.md", md.ToString());
            }

            string[] mds = Directory.GetFiles(dir, "*.md");

            if (dir.Contains("Changelogs"))
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
            //string openClass = active == ActiveState.Child ? "open" : "";
            html.AppendLine($"<li class=\"{activeClass}\">");
 
            {
                string link = n.Link.Replace(Nav.RootPath, string.Empty).Replace("\\", "/").Replace(".md", ".html");
                html.AppendLine($"<a href=\"{link}\">{n.Title}</a>");
            }

            html.AppendLine("</li>");

            return html.ToString();
        }
    }


}