using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngryMonkey.Objects;
using Newtonsoft.Json;

namespace AngryMonkey
{
    internal partial class Documenter
    {
        public string RootPath { get; set; }

        public List<Hive> Hives { get; set; } = new List<Hive>();

        public void ProcessHives()
        {
            SetConsoleAppearance();

            Console.WriteLine("          __\r\n     w  c(..)o   (\r\n      \\__(-)    __)\r\n          /\\   (\r\n         /(_)___)\r\n         w /|\r\n          | \\\r\n         m  m");

            if (Hives == null || Hives.Count == 0)
            {
                if (File.Exists(RootPath + "\\hives.json"))
                {
                    Hives = JsonConvert.DeserializeObject<List<Hive>>(File.ReadAllText(RootPath + "\\hives.json"));
                }
                else
                {
                    WriteLine("No HIVES were specified. HIVES.JSON is missing from the root.", sharp);
                    WriteLine("\nThe monkey is angry!\nBUILD CANCELLED", sharp);
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
                            WriteLine("\n(WARNING)\nMissing UIDs!", attn);
                        WriteLine($"    {item.Title} ({item.Link})", info);

                        bad = true;
                    }

                    uids.Add(item.UID);
                }

                int duplicates = uids.Count - uids.Distinct().Count();
                if (duplicates > 0)
                {
                    WriteLine($"\n(ERROR)\n{duplicates} duplicate(s) found!", sharp);
                    List<string> dupes = uids.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                             .Select(y => y.Key)
                                             .ToList();
                    foreach (string dupe in dupes.Distinct())
                    {
                        WriteLine($"    {dupe ?? "<null>"}", attn);
                        foreach (NavItem n in Nav.identifiers.Where(nn => nn.UID == dupe))
                        {
                            WriteLine($"       {n.Title} :: {n.Link}", info);
                        }
                    }

                    WriteLine("\nThe monkey is angry!\nBUILD CANCELLED", sharp);
                    return;
                }

                if (!bad)
                {
                    OK();
                    Write(Nav.identifiers.Count.ToString(), attn);
                    WriteLine(" unique identifiers.", dim);
                    Write(Nav.identifiers.Count(n => !n.Show).ToString(), attn);
                    WriteLine(" hidden identifiers.", dim);
                }
            }

            WriteLine("", dim);

            DrawLine("Hives", info);

        

            foreach (Hive hive in Hives)
            {
                Write($"Processing {hive.Path.ToUpper()}...", dim);
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

            WriteLine("", dim);
            WriteLine("The monkey is happy!\nGENERATION COMPLETE", success);
            WriteLine("", dim);
        }

        private static void OK() { WriteLine("OK", success); }
    }
}