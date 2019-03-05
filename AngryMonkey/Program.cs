using System;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            string root = @"Z:\Git\Gaea\Gaea-Docs\";
            if (args.Length == 0)
            {
                root = Environment.CurrentDirectory;
            }

            if (!root.EndsWith("\\"))
                root = root + "\\";

            Documenter doc = new Documenter
            {
                RootPath = root
                //! Specify manually, otherwise it will load from JSON
                //new List<Hive>
                //{
                //    new Hive("USER GUIDE", new NavItem("User Guide", "guide.html") { UID = "guide" }),
                //    new Hive("REFERENCE", new NavItem("Reference", "reference.html") { UID = "reference" }, true, true),
                //    new Hive("TUTORIALS", new NavItem("Tutorials", "tutorials.html") { UID = "tutorials" }),
                //    new Hive("DEVELOPERS", new NavItem("Developers", "developers.html") { UID = "dev" })
                //}
            };

            doc.ProcessHives();
        }
    }
}