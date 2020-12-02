using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using AngryMonkey.Objects;

#pragma warning disable 618

namespace AngryMonkey
{
    public partial class Processor
    {
        private static MD5 md5 = MD5.Create();

        private static string Strip(string name)
        {
            if (name.Contains("-"))
            {
                string prefix = name.Split('-').First();
                if (int.TryParse(prefix, out int _))
                {
                    return name.Replace($"{prefix}-", string.Empty);
                }
            }

            return name;
        }

        public void Write(string text, bool appendLine = false, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            if (appendLine)
                Console.WriteLine(text);
            else
                Console.Write(text);
        }

        public void OK() => Write("OK!", true, ConsoleColor.Green);

        private bool IsStale(Page page)
        {
            if (hashes.ContainsKey(page.Filename))
                return GetMD5(page) != hashes[page.Filename];

            return true;
        }        
        
        private bool IsStale(string name, string contents)
        {
            if (hashes.ContainsKey(name))
            {
                return GetMD5(contents) != hashes[name];
            }

            return true;
        }

        private void LoadHashes()
        {
            if (File.Exists(Source + "\\hash.bin"))
            {
                Write("\n   Loading hashes...", false, ConsoleColor.Yellow);
                using var fs = new FileStream(Source + "\\hash.bin", FileMode.Open, FileAccess.Read);
                try
                {
                    hashes = new BinaryFormatter().Deserialize(fs) as Dictionary<string, string>;
                    OK();
                }
                catch (Exception)
                {
                    Write("ERROR!.", true, ConsoleColor.Red);
                }
            }
            else
            {
                Write("\n   Hashes not found.", true, ConsoleColor.Yellow);
            }
        }

        private void SaveHashes()
        {
            using var fs = new FileStream(Source + "\\hash.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            try
            {
                new BinaryFormatter().Serialize(fs, hashes);
            }
            catch (Exception)
            {
                Write("\n   ERROR! Hashes could not be saved.", true, ConsoleColor.Red);
            }
        }

        public static string GetMD5(Page page) => Encoding.Default.GetString(md5.ComputeHash(Encoding.Default.GetBytes(page.Contents)));
        public static string GetMD5(string text) => Encoding.Default.GetString(md5.ComputeHash(Encoding.Default.GetBytes(text)));

        public static string CreateMd5ForFolder(string path)
        {
            // assuming you want to include nested folders
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .OrderBy(p => p).ToList();

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