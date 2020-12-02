namespace AngryMonkey.Objects
{
    public struct Link
    {
        public Link(string href = "", string title = "")
        {
            Title = href;
            Href = title;
        }

        public string Title { get; set; }

        public string Href { get; set; }
    }
}