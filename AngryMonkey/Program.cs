using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Processor guide = new Processor(@"Z:\Git\Gaea\Gaea-Docs\")
                              {
                                  src = @"source\USER GUIDE\",
                                  RootPath = @"Z:\Git\Gaea\Gaea-Docs\"
                              };

            string[] delete = Directory.GetFiles(guide.RootPath + guide.dst, "*.html", SearchOption.TopDirectoryOnly);

            foreach (string d in delete)
            {
                File.Delete(d);
            }
            Console.Write("Mapping identifiers...");
            guide.GetIdentifiers(guide.RootPath + "source\\");
            {
                List<string> uids = new List<string>();
                bool bad = false;
                foreach (NavItem item in guide.identifiers)
                {
                    if (item.UID == null)
                    {
                        if (!bad)
                            Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"    {item.Title}({item.Link}) --- MISSING UID!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        bad = true;
                    }
                    uids.Add(item.UID);
                }
                int duplicates = uids.Count - uids.Distinct().Count();
                if (duplicates > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n{duplicates} duplicates found!");
                    List<string> dupes = uids.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                             .Select(y => y.Key)
                                             .ToList();
                    foreach (string dupe in dupes.Distinct())
                    {
                        Console.WriteLine($"    {dupe ?? "<null>"}");
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("\nThe monkey is angry! Build cancelled.");
                    return;
                }
                if (!bad)
                {
                    OK();
                }
            }
            Console.Write("Parsing USER GUIDE...");
            guide.Process();
            guide.ProcessChangelogs();
            // guide.ProcessLooseFiles(@"source\USER GUIDE_loose\");
            OK();
            Processor reference = new Processor(@"Z:\Git\Gaea\Gaea-Docs\")
            {
                src = @"source\REFERENCE\",
                identifiers = guide.identifiers,
                BaseItem = new NavItem("Reference", "reference.html") { UID = "reference" }
            };
            Console.Write("Parsing REFERENCE...");
            reference.Process();
            OK();
            Processor tuts = new Processor(@"Z:\Git\Gaea\Gaea-Docs\")
            {
                src = @"source\tutorials\",
                identifiers = guide.identifiers,
                BaseItem = new NavItem("Tutorials", "tutorials.html") { UID = "tutorials" }
            };
            Console.Write("Parsing TUTS...");
            tuts.Process();
            OK();
            Processor developers = new Processor(@"Z:\Git\Gaea\Gaea-Docs\")
            {
                src = @"source\DEVELOPERS\",
                identifiers = guide.identifiers,
                BaseItem = new NavItem("Developers", "developers.html") { UID = "dev" }
            };
            Console.Write("Parsing DEV...");
            developers.Process();
            OK();
            Processor unsorted = new Processor(@"Z:\Git\Gaea\Gaea-Docs\")
            {
                src = @"source\unsorted\",
                identifiers = guide.identifiers,
                BaseItem = new NavItem("Home", "index.html") { UID = "index" }
            };
            Console.Write("Parsing UNSORTED...");
            unsorted.Process();
            OK();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{guide.identifiers.Count} unique identifiers mapped.");
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done.\n");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        private static void OK()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}