using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AngryMonkey
{
    internal static class Nav
    {
        internal static TextInfo tt = new CultureInfo("en-US", false).TextInfo;
        internal static List<NavItem> identifiers = new List<NavItem>();

        internal static void GetIdentifiers(string dir)
        {
            identifiers.Clear();
            IEnumerable<string> mds = Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories);
       
            foreach (string md in mds.Where(md => !md.ToLower().EndsWith(".params.md") && !md.Contains("--")))
            {
                identifiers.Add(GetNavItem(md));
            }
        }

        internal static NavItem GetNavItem(string md)
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

        internal static string SanitizeFilename(string md)
        {
            return Path.GetFileNameWithoutExtension(md).Contains("-") &&
                   int.TryParse(Path.GetFileNameWithoutExtension(md).Split('-')[0], out int _)
                       ? Path.GetFileNameWithoutExtension(md)
                             .Replace(Path.GetFileNameWithoutExtension(md).Split('-')[0] + "-", string.Empty)
                       : Path.GetFileNameWithoutExtension(md);
        }
    }
}