using System.Collections.Generic;
using AngryMonkey.Objects;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Documenter doc = new Documenter
                             {
                RootPath = @"Z:\Git\Gaea\Gaea-Docs\",
                Hives = new List<Hive>
                        {
                            new Hive("USER GUIDE", new NavItem("User Guide", "guide.html") { UID = "guide" }),
                            new Hive("REFERENCE", new NavItem("Reference", "reference.html") { UID = "reference" }, true, true),
                            new Hive("TUTORIALS", new NavItem("Tutorials", "tutorials.html") { UID = "tutorials" }),
                            new Hive("DEVELOPERS", new NavItem("Developers", "developers.html") { UID = "dev" })
                        }
            };

            doc.ProcessHives();
        }
    }
}