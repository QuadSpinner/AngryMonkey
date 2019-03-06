using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AngryMonkey.Objects;
using Newtonsoft.Json;

namespace AngryMonkey
{
    internal class Documenter
    {
        public List<Hive> Hives { get; set; } = new List<Hive>();

        public void ProcessHives()
        {
            C.SetConsoleAppearance();

            Console.WriteLine("          __\r\n     w  c(..)o   (\r\n      \\__(-)    __)\r\n          /\\   (\r\n         /(_)___)\r\n         w /|\r\n          | \\\r\n         m  m", C.gray);

            if (MapIdentifiers()) return;

            if (Hives == null || Hives.Count == 0)
            {
                if (File.Exists(Nav.RootPath + "\\hives.json"))
                {
                    Hives = JsonConvert.DeserializeObject<List<Hive>>(File.ReadAllText(Nav.RootPath + "\\hives.json"));
                    C.Write("\nROOT: ", C.gray);
                    C.WriteLine(Nav.RootPath, C.gold);
                }
                else
                {
                    C.WriteLine("No HIVES were specified. HIVES.JSON is missing from the root.", C.red);
                    C.WriteLine("\nThe monkey is angry!\nBUILD CANCELLED", C.red);
                    return;
                }
            }

            C.WriteLine("", C.gray);

            C.DrawLine("Hives", C.blue);

            if (File.Exists(Nav.RootPath + "source\\index.md"))
            {
                C.Write("Processing Root Document...", C.gray);
                StringBuilder superNav = new StringBuilder();
                foreach (Hive h in Hives)
                    superNav.AppendLine($"<div class=\"nav-item\"><a class=\"nav-link\" href=\"/{h.BaseItem.Link}\">{h.BaseItem.Title}</a></div>");

                Processor p = new Processor(Nav.RootPath) { MainNavHTML = superNav.ToString() };
                p.ProcessRootMD(Nav.RootPath + "source\\index.md");
                OK();
            }

            foreach (Hive hive in Hives)
            {
                C.Write($"Processing {hive.Path}...", C.gray);

                string path = Nav.RootPath + "docs\\" + hive.Path;
                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                catch (Exception)
                {
                    C.WriteLine("\n(ERROR) Existing directory " + path + " could not be deleted.\n\nSKIPPING THIS HIVE!", C.red);
                    continue;
                }

                StringBuilder superNav = new StringBuilder();
                foreach (Hive h in Hives)
                {
                    if (h.Path == hive.Path)
                        superNav.AppendLine($"<div class=\"nav-item active\"><a class=\"nav-link\" href=\"/{h.BaseItem.Link}\">{h.BaseItem.Title}</a></div>");
                    else
                        superNav.AppendLine($"<div class=\"nav-item\"><a class=\"nav-link\" href=\"/{h.BaseItem.Link}\">{h.BaseItem.Title}</a></div>");
                }

                Processor p = new Processor(Nav.RootPath)
                {
                    MainNavHTML = superNav.ToString(),
                    src = @"source\" + hive.Path,
                    BaseItem = hive.BaseItem,
                    ProcessProceduralFiles = hive.ProcessProceduralFiles,
                    ProcessExampleFiles = hive.ProcessExampleFiles
                };
                p.Process();

                OK();
            }

            C.WriteLine("", C.gray);
            C.WriteLine("The monkey is happy!\nGENERATION COMPLETE", C.green);
            C.WriteLine("", C.gray);
        }

        private static bool MapIdentifiers()
        {
            if (Nav.identifiers.Count == 0)
            {
                C.DrawLine("Identifiers", C.blue);

                C.Write("Mapping identifiers...", C.gray);
                Nav.GetIdentifiers(Nav.RootPath + "source\\");

                List<string> uids = new List<string>();
                bool bad = false;
                foreach (NavItem item in Nav.identifiers)
                {
                    if (item.UID == null)
                    {
                        if (!bad)
                            C.WriteLine("\n(WARNING)\nMissing UIDs!", C.gold);

                        C.WriteLine($"    {item.Title} ({item.Link})", C.blue);
                        bad = true;
                    }

                    uids.Add(item.UID);
                }

                int duplicates = uids.Count - uids.Distinct().Count();
                if (duplicates > 0)
                {
                    C.WriteLine($"\n(ERROR)\n{duplicates} duplicate(s) found!", C.red);
                    List<string> dupes = uids.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                             .Select(y => y.Key)
                                             .ToList();

                    foreach (string dupe in dupes.Distinct())
                    {
                        C.WriteLine($"    {dupe ?? "<null>"}", C.gold);
                        foreach (NavItem n in Nav.identifiers.Where(nn => nn.UID == dupe))
                        {
                            C.WriteLine($"       {n.Title} :: {n.Link}", C.blue);
                        }
                    }

                    C.WriteLine("\nThe monkey is angry!\nBUILD CANCELLED", C.red);
                    return true;
                }

                if (!bad)
                {
                    OK();
                    C.Write(Nav.identifiers.Count.ToString(), C.gold);
                    C.WriteLine(" unique identifiers.", C.gray);
                    C.Write(Nav.identifiers.Count(n => !n.Show).ToString(), C.gold);
                    C.WriteLine(" hidden identifiers.", C.gray);
                }
            }

            return false;
        }

        private static void OK() { C.WriteLine("OK", C.green); }
    }
}