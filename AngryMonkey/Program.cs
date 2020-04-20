using System;
using System.Text;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            string root = Environment.CurrentDirectory; // Use current directory

            //if (args.Length > 0 && args[1] != "--force")
            //    root = args[1]; // Use path supplied in the arguments

            if (!root.EndsWith("\\"))
                root = $"{root}\\";

            Nav.RootPath = root;

            Documenter doc = new Documenter();
            doc.ProcessHives();
        }
    }


    public static class Logger{
        public static StringBuilder atLogs = new StringBuilder();
    }
}