namespace AngryMonkey.Objects
{
    public class Hive
    {
        public Hive(string path, NavItem nav, bool procedural = false, bool examples = false)
        {
            Path = path;
            BaseItem = nav;
            ProcessProceduralFiles = procedural;
            ProcessExampleFiles = examples;
        }

        public string Path { get; set; }

        public NavItem BaseItem { get; set; }

        public bool ProcessProceduralFiles { get; set; } = true;

        public bool ProcessExampleFiles { get; set; } = true;
    }
}