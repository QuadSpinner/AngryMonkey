using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AngryMonkey
{

    enum ActiveState
    {
        None,
        Self,
        Child
    }
    internal static class Nav
    {
        internal static TextInfo tt = new CultureInfo("en-US", false).TextInfo;
        internal static List<NavItem> identifiers = new List<NavItem>();
        public static string RootPath { get; set; }

        internal static void GetIdentifiers(string dir)
        {
            identifiers.Clear();
            IEnumerable<string> mds = Directory.GetFiles(dir, "*.md", SearchOption.AllDirectories);

            foreach (string md in mds.Where(md => !md.ToLower().EndsWith(".params.md") && !md.Contains("--") && !md.Contains("\\_")))
            {
                identifiers.Add(GetNavItem(md));
            }
        }

        internal static NavItem GetNavItem(string md)
        {
            string name = SanitizeFilename(md);

            NavItem n = new NavItem(tt.ToTitleCase(name.Replace("-", " ")), md);

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

            }

            n.Link = Uri.EscapeUriString(ReplaceNumbers(n.Link.Replace(RootPath + "source", string.Empty).Replace("\\", "/").Replace(".md", ".html")));

            return n;
        }

        internal static string ReplaceNumbers(string subpath)
        {
            for (int i = 0; i < 100; i++)
            {
                subpath = subpath.Replace($"{i:000}-", "");
            }

            for (int i = 0; i < 100; i++)
            {
                subpath = subpath.Replace($"{i:00}-", "");
            }

            for (int i = 0; i < 100; i++)
            {
                subpath = subpath.Replace($"{i}-", "");
            }

            return subpath;
        }

        internal static string SanitizeFilename(string md)
        {
            return Path.GetFileNameWithoutExtension(md).Contains("-") &&
                   int.TryParse(Path.GetFileNameWithoutExtension(md).Split('-')[0], out int _)
                       ? Path.GetFileNameWithoutExtension(md)
                             .Replace(Path.GetFileNameWithoutExtension(md).Split('-')[0] + "-", String.Empty)
                       : Path.GetFileNameWithoutExtension(md);
        }
    }
}