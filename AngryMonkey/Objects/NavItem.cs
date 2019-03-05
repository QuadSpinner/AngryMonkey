using System.Collections.Generic;

namespace AngryMonkey
{
    public class NavItem
    {
        public NavItem()
        {
        }

        public NavItem(string title) => Title = title;

        public NavItem(string title, string link)
        {
            Title = title;
            Link = link;
        }

        public string Title { get; set; }

        public string Link { get; set; }

        public string UID { get; set; }

        public List<NavItem> Items { get; set; } = new List<NavItem>();

        public override string ToString() => Title;
    }
}