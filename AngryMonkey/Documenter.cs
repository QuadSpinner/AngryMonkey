using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            if (MapIdentifiers()) return;

            if (Hives == null || Hives.Count == 0)
            {
                if (File.Exists(Nav.RootPath + "\\hives.json"))
                {
                    Hives = JsonConvert.DeserializeObject<List<Hive>>(File.ReadAllText(Nav.RootPath + "\\hives.json"));
                    Console.Write("\nROOT: ");
                    Console.WriteLine(Nav.RootPath);
                }
                else
                {
                    Console.WriteLine("No HIVES were specified. HIVES.JSON is missing from the root.");
                    Console.WriteLine("\nThe monkey is angry!\nBUILD CANCELLED");
                    return;
                }
            }

            if (File.Exists(Nav.RootPath + "source\\index.md"))
            {
                Console.Write("Processing Root Document...");
                StringBuilder superNav = new StringBuilder();
                foreach (Hive h in Hives)
                    superNav.AppendLine(
                        $"<li class=\"nav-item\"><a class=\"nav-link\" href=\"/{h.BaseItem.Link}\">{h.BaseItem.Title}</a></li>");

                Processor p = new Processor(Nav.RootPath) { MainNavHTML = superNav.ToString() };
                p.ProcessRootMD(Nav.RootPath + "source\\index.md");
                OK();
            }

            foreach (Hive hive in Hives)
            {
                Console.Write($"Processing {hive.Path}...");

                string path = Nav.RootPath + "docs\\" + hive.Path;

                string newMD5 = CreateMd5ForFolder(Nav.RootPath + "source\\" + hive.Path);
                
                if (File.Exists(Nav.RootPath + "source\\" + hive.Path + "_hash.md5"))
                {
                    if (newMD5 == File.ReadAllText(Nav.RootPath + "source\\" + hive.Path + "_hash.md5"))
                    {
                        Console.WriteLine("HASH SKIP!");
                        continue;
                    }
                }

                File.WriteAllText(Nav.RootPath + "source\\" + hive.Path + "_hash.md5", newMD5);


                try
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                }
                catch (Exception)
                {
                    Console.WriteLine("\n(ERROR) Existing directory " + path +
                                      " could not be deleted.\n\nSKIPPING THIS HIVE!");
                    continue;
                }

                StringBuilder superNav = new StringBuilder();
            
                foreach (Hive h in Hives)
                {

                    superNav.AppendLine(h.Path == hive.Path
                                            ? $"<li class=\"active\"><a href=\"/{h.BaseItem.Link}\">{h.BaseItem.Title}</a></li>"
                                            : $"<li><a href=\"/{h.BaseItem.Link}\">{h.BaseItem.Title}</a></li>");
                }
              

                Processor p = new Processor(Nav.RootPath)
                {
                    MainNavHTML = superNav.ToString(),
                    src = @"source\" + hive.Path,
                    BaseItem = hive.BaseItem,
                    ProcessProceduralFiles = hive.ProcessProceduralFiles,
                    ProcessExampleFiles = hive.ProcessExampleFiles,
                    ProcessTutorialFiles = hive.ProcessTutorialFiles
                };
                p.Process();

                OK();
            }

            Console.WriteLine("The monkey is happy!\nGENERATION COMPLETE");
        }

        private static bool MapIdentifiers()
        {
            if (Nav.identifiers.Count == 0)
            {
                Console.Write("Mapping identifiers...");
                Nav.GetIdentifiers(Nav.RootPath + "source\\");

                List<string> uids = new List<string>();
                bool bad = false;
                foreach (NavItem item in Nav.identifiers)
                {
                    if (item.UID == null)
                    {
                        if (!bad)
                            Console.WriteLine("\n(WARNING)\nMissing UIDs!");

                        Console.WriteLine($"    {item.Title} ({item.Link})");
                        bad = true;
                    }

                    uids.Add(item.UID);
                }

                int duplicates = uids.Count - uids.Distinct().Count();
                if (duplicates > 0)
                {
                    Console.WriteLine($"\n(ERROR)\n{duplicates} duplicate(s) found!");
                    List<string> dupes = uids.GroupBy(x => x)
                                             .Where(g => g.Count() > 1)
                                             .Select(y => y.Key)
                                             .ToList();

                    foreach (string dupe in dupes.Distinct())
                    {
                        Console.WriteLine($"    {dupe ?? "<null>"}");
                        foreach (NavItem n in Nav.identifiers.Where(nn => nn.UID == dupe))
                        {
                            Console.WriteLine($"       {n.Title} :: {n.Link}");
                        }
                    }

                    Console.WriteLine("\nThe monkey is angry!\nBUILD CANCELLED");
                    return true;
                }

                if (!bad)
                {
                    OK();
                    Console.Write(Nav.identifiers.Count.ToString());
                    Console.WriteLine(" unique identifiers.");
                    Console.Write(Nav.identifiers.Count(n => !n.Show).ToString());
                    Console.WriteLine(" hidden identifiers.");
                }
            }

            return false;
        }

        private static void OK() { Console.WriteLine("OK"); }

        public static string CreateMd5ForFolder(string path)
        {
            // assuming you want to include nested folders
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .OrderBy(p => p).ToList();

            MD5 md5 = MD5.Create();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];

                // hash path
                string relativePath = file.Substring(path.Length + 1);
                byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                byte[] contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }
    }
}