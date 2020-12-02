using System;

namespace AngryMonkey
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "AngryMonkey";
            
            string root = Environment.CurrentDirectory; // Use current directory

            if (!root.EndsWith("\\"))
                root = $"{root}\\";

            new Processor
            {
                Source = root + "source",
                Destination = root + "docs",
                Force = Environment.CommandLine.Contains("--force")
            }.Process();
        }
    }
}