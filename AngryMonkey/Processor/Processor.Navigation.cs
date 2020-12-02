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

            PreprocessChangelogs(dir);

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

        private static void PreprocessChangelogs(string dir)
        {
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
        }

        private void CreateNavigation()
        {
            StringBuilder nhtml = new StringBuilder();
            StringBuilder ohtml = new StringBuilder();

            foreach (NavItem item in navs.Items)
            {
                //ActiveState active = ActiveState.None;
                //string uid = Nav.GetNavItem(MDs[i]).UID;
                string nid = Nav.SanitizeFilename(item.Title).Replace(" ", string.Empty);

                nhtml.AppendLine(
                    $"<li class=\"panel collapsed\"><a class=\"area\" href=\"#{nid}\" data-parent=\"#main-nav\" data-toggle=\"collapse\">{item.Title}</a>");
                nhtml.AppendLine($"<ul id=\"{nid}\" class=\"collapse\">");


                ohtml.AppendLine($"<optgroup label=\"{item.Title}\">");

                foreach (NavItem navItem in item.Items)
                {
                    nhtml.AppendLine(
                        $"<li class=\"xref\"><a href=\"{navItem.Link.Replace(Nav.RootPath, string.Empty).Replace("\\", "/").Replace(".md", ".html")}\">{navItem.Title}</a></li>");
                    ohtml.AppendLine($"<option value=\"{navItem.Link}\">{navItem.Title}</option>");
                }

                nhtml.AppendLine("</ul></li></div>");
                ohtml.AppendLine("</optgroup>");
            }

            navHtml = minifier.Minify(nhtml.ToString()).MinifiedContent;
            optHtml = "<select id=\"small-nav-dropdown\">" + minifier.Minify(ohtml.ToString()).MinifiedContent + "</select>";

            File.WriteAllText($"{dst}navs\\{BaseItem.UID}-n.html", navHtml);
            File.WriteAllText($"{dst}navs\\{BaseItem.UID}-o.html", optHtml);
        }
    }


}