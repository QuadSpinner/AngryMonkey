using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;

namespace AngryMonkey.Objects
{
    public class Page
    {
        public string Title { get; set; }

        public string Href =>  $"/{Hive.Path}/{Parent.Href}/{Strip(Path.GetFileNameWithoutExtension(Filename))}.html";

        public string Filename { get; set; }

        public string UID { get; set; }

        public Hive2 Hive { get; set; }

        public Link Parent { get; set; }

        public bool Hidden { get; set; }

        public string Contents { get; set; }

        private static string Strip(string name)
        {
            if (name.Contains("-"))
            {
                string prefix = name.Split('-').First();
                if (int.TryParse(prefix, out int _))
                {
                    return name.Replace($"{prefix}-", string.Empty);
                }
            }
            return name;
        }
        public SearchObject ToSearchObject()
        {
            string[] doc = Regex.Replace(Regex.Replace(Markdown.ToPlainText(Contents), @"\{([^\}]+)\}", string.Empty),
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
                                .Trim().Split(' ', StringSplitOptions.TrimEntries);

            StringBuilder temp = new StringBuilder();
            foreach (string s in doc.Distinct())
            {
                temp.Append($"{s} ");
            }

            return new SearchObject
            {
                hive = Hive.Title,
                text = temp.ToString(),
                title = Title,
                url = Href
            };
        }
    }
}