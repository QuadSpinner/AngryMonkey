using System;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            string root = args.Length == 0 
                              ? Environment.CurrentDirectory // Use current directory
                              : args[1]; // Use path supplied in the arguments

            if (!root.EndsWith("\\"))
                root = $"{root}\\";

            Nav.RootPath = root;

            Documenter doc = new Documenter();
            doc.ProcessHives();
        }
    }
}