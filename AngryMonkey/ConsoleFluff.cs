using System;
using System.Drawing;
using System.Threading;
using Console = Colorful.Console;

namespace AngryMonkey
{
    internal class C
    {
        private const string Title = "QuadSpinner AngryMonkey";
        internal static Color gray = Color.Gray;
        internal static Color green = Color.PaleGreen;
        internal static Color gold = Color.PaleGoldenrod;
        internal static Color blue = Color.PaleTurquoise;
        internal static Color purple = Color.MediumSlateBlue;
        internal static Color red = Color.PaleVioletRed;

        private static int animationSpeed { get; } = 5;

        private static int waitSpeed { get; } = 30;

        internal static void SetConsoleAppearance()
        {
            Console.Title = Title;

            Console.BackgroundColor = Color.FromArgb(255, 33, 33, 33);

            Console.Write(".", green);
            Console.Write(".", gold);
            Console.Write(".", blue);
            Console.Write(".", purple);
            Console.Write(".", red);
            Console.Write(".", gray);
            Console.WindowHeight = Math.Min(40, Console.LargestWindowHeight);
            Console.WindowWidth = Math.Min(100, Console.LargestWindowHeight);
            Console.Clear();

            DrawTitle(Title, gray);
        }

        internal static void DrawLine(string section)
        {
            section = " " + section + " ";
            Console.WriteLine();
            WriteLine("".PadLeft(5, '─') + section.PadRight(Console.WindowWidth - 7, '─'), purple);
            Console.WriteLine();
        }

        internal static void DrawLine(string section, Color color)
        {
            section = " " + section + " ";
            Console.WriteLine();
            WriteLine("".PadLeft(5, '─') + section.PadRight(Console.WindowWidth - 7, '─'), color);
            Console.WriteLine();
        }

        internal static void DrawTitle(string section, Color color, char c = '─')
        {
            section = " " + section + " ";
            Console.WriteLine();
            Write("".PadLeft(5, c), color);
            Console.WriteWithGradient(section.ToCharArray(), red, gold, 8);
            WriteLine("".PadRight(Console.WindowWidth - 7 - section.Length, c), color);
            Console.WriteLine();
        }

        internal static string FixedText(string s, int size)
        {
            if (s.Length > size)
                s = s.Substring(0, size - 3) + "...";

            if (s.Length < size)
                s = s.PadRight(size, ' ');

            return s;
        }

        internal static void EraseLines(int lines)
        {
            Console.CursorLeft = 0;
            Console.CursorTop -= lines;
            for (int i = 0; i < lines; i++)
            {
                Console.WriteLine("".PadLeft(Console.WindowWidth - 1));
            }

            Console.CursorTop -= lines;
            Console.CursorLeft = 0;
        }

        internal static void WriteLine(string a, Color c1, bool wait = true)
        {
            Write(a, "", c1, c1, wait);
            Console.WriteLine();
        }

        internal static void Write(string a, Color c1, bool wait = true) => Write(a, "", c1, c1, wait);

        internal static void WriteLine(string a, string b, Color c1, Color c2, bool wait = true)
        {
            Write(a, b, c1, c2, wait);
            Console.WriteLine();
        }

        internal static void Write(string a, string b, Color c1, Color c2, bool wait = true)
        {
            Console.Write(a, c1);
            Console.Write(b, c2);

            if (wait)
                Thread.Sleep(waitSpeed);
        }

        internal static void DrawProgressBar(int complete,
                                            int maxVal,
                                            string prefix,
                                            string prefixColored = "",
                                            bool showPbefore = false,
                                            int barSize = 32,
                                            char progressCharacter = '=')
        {
            try
            {
                System.Console.CursorVisible = false;
                int left = System.Console.CursorLeft;
                decimal perc = complete / (decimal)(maxVal - 1);
                int chars = (int)Math.Floor(perc / (1 / (decimal)barSize));
                string p1 = string.Empty;
                string p2 = string.Empty;

                p1 = p1.PadLeft(chars, progressCharacter);
                p2 = p2.PadLeft(Math.Max(barSize - chars, 0), ' ');

                Console.Write(prefix + " ", gray);
                Console.Write(prefixColored + " ", chars >= barSize ? gray : blue);

                if (showPbefore)
                    Console.Write($" {perc * 100:##0}%", chars >= barSize ? gray : gold);

                Console.Write("[", gray);
                Console.Write(p1, chars >= barSize ? gray : green);
                Console.Write(p2, gray);
                Console.Write("]", gray);

                if (!showPbefore)
                    Console.Write($" {perc * 100:##0}%", chars >= barSize ? gray : gold);

                //System.Console.ResetColor();
                System.Console.CursorLeft = left;
            }
            catch (Exception) { }
        }
    }
}