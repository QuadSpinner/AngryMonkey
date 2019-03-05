using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngryMonkey.Objects;

namespace AngryMonkey
{
    internal partial class Documenter
    {
        public string RootPath { get; set; }

        public List<Hive> Hives { get; set; } = new List<Hive>();

        public void ProcessHives()
        {
            SetConsoleAppearance();

            string[] delete = Directory.GetFiles(RootPath + "docs", "*.html", SearchOption.TopDirectoryOnly);

            foreach (string d in delete)
            {
                File.Delete(d);
            }

            if (Nav.identifiers.Count == 0)
            {
                DrawLine("Identifiers", info);

                Write("Mapping identifiers...", dim);
                Nav.GetIdentifiers(RootPath + "source\\");

                List<string> uids = new List<string>();
                bool bad = false;
                foreach (NavItem item in Nav.identifiers)
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
                    WriteLine($"{Nav.identifiers.Count} unique identifiers.", dim);
                    WriteLine($"{Nav.identifiers.Count(n => !n.Show)} hidden identifiers.", dim);
                }
            }

            WriteLine("", dim);

            DrawLine("Generating hives", info);

            foreach (Hive hive in Hives)
            {
                Write($"Processing {hive.Path.ToUpper()}...", dim);
                Processor p = new Processor(RootPath)
                              {
                                  src = @"source\" + hive.Path,
                                  BaseItem = hive.BaseItem,
                                  ProcessProceduralFiles =hive.ProcessProceduralFiles,
                                  ProcessExampleFiles = hive.ProcessExampleFiles
                              };
                p.Process();
                OK();
            }

            WriteLine("", dim);
            WriteLine("Generation completed.", success);
            WriteLine("", dim);
        }

        private static void OK() { WriteLine("OK", success); }
    }
}