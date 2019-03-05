using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngryMonkey.Objects;
using Newtonsoft.Json;

namespace AngryMonkey
{
    internal class Documenter
    {
        public string RootPath { get; set; }

        public List<Hive> Hives { get; set; } = new List<Hive>();

        public void ProcessHives()
        {
            C.SetConsoleAppearance();

            C.WriteLine("          __\r\n     w  c(..)o   (\r\n      \\__(-)    __)\r\n          /\\   (\r\n         /(_)___)\r\n         w /|\r\n          | \\\r\n         m  m", C.gray);

            if (Hives == null || Hives.Count == 0)
            {
                if (File.Exists(RootPath + "\\hives.json"))
                {
                    Hives = JsonConvert.DeserializeObject<List<Hive>>(File.ReadAllText(RootPath + "\\hives.json"));
                    C.Write("\nROOT: ", C.gray);
                    C.WriteLine(RootPath, C.gold);
                }
                else
                {
                    C.WriteLine("No HIVES were specified. HIVES.JSON is missing from the root.", C.red);
                    C.WriteLine("\nThe monkey is angry!\nBUILD CANCELLED", C.red);
                    return;
                }
            }

            string[] delete = Directory.GetFiles(RootPath + "docs", "*.html", SearchOption.TopDirectoryOnly);

            foreach (string d in delete)
            {
                File.Delete(d);
            }

            if (Nav.identifiers.Count == 0)
            {
                C.DrawLine("Identifiers", C.blue);

                C.Write("Mapping identifiers...", C.gray);
                Nav.GetIdentifiers(RootPath + "source\\");

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
                    return;
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

            C.WriteLine("", C.gray);

            C.DrawLine("Hives", C.blue);

            foreach (Hive hive in Hives)
            {
                C.Write($"Processing {hive.Path}...", C.gray);
                Processor p = new Processor(RootPath)
                {
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

        private static void OK() { C.WriteLine("OK", C.green); }
    }
}