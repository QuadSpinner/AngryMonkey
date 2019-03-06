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
                root = $"{root}\\";

            Nav.RootPath = root;

            Documenter doc = new Documenter();
            doc.ProcessHives();
        }
    }
}