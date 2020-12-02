using System;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "AngryMonkey";


            string root = "Z:\\Git\\Gaea\\Gaea-Docs"; // Environment.CurrentDirectory; // Use current directory

            //if (args.Length > 0 && args[1] != "--force")
            //    root = args[1]; // Use path supplied in the arguments

            if (!root.EndsWith("\\"))
                root = $"{root}\\";

            new Processor2
            {
                Source = root + "source",
                Destination = root + "docs",
                Force = Environment.CommandLine.Contains("--force")
            }.Process();

            //Nav.RootPath = root;

            //Documenter doc = new Documenter();
            //doc.ProcessHives();
        }
    }
}