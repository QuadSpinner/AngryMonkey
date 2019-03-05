using System;
using System.Drawing;
using System.Threading;
using Console = Colorful.Console;

namespace AngryMonkey
{
    internal partial class Documenter
    {
        private const string Title = "QuadSpinner AngryMonkey";
        private static Color dim = Color.Gray;
        private static Color success = Color.PaleGreen;
        private static Color attn = Color.PaleGoldenrod;
        private static Color info = Color.PaleTurquoise;
        private static Color focus = Color.MediumSlateBlue;
        private static Color sharp = Color.PaleVioletRed;

        private static int animationSpeed { get; } = 3;

        private static int waitSpeed { get; } = 300;

        private static void SetConsoleAppearance()
        {
            Console.Title = Title;

            Console.BackgroundColor = Color.FromArgb(255, 33, 33, 33);

            Console.Write(".", success);
            Console.Write(".", attn);
            Console.Write(".", info);
            Console.Write(".", focus);
            Console.Write(".", sharp);
            Console.Write(".", dim);

            Console.Clear();

            DrawTitle(Title, dim);
        }

        private static void DrawLine(string section)
        {
            section = " " + section + " ";
            Console.WriteLine();
            WriteLine("".PadLeft(5, '─') + section.PadRight(Console.WindowWidth - 7, '─'), focus);
            Console.WriteLine();
        }

        private static void DrawLine(string section, Color color)
        {
            section = " " + section + " ";
            Console.WriteLine();
            WriteLine("".PadLeft(5, '─') + section.PadRight(Console.WindowWidth - 7, '─'), color);
            Console.WriteLine();
        }

        private static void DrawTitle(string section, Color color, char c = '─')
        {
            section = " " + section + " ";
            Console.WriteLine();
            Write("".PadLeft(5, c), color);
            Console.WriteWithGradient(section.ToCharArray(), sharp, attn, 8);
            WriteLine("".PadRight(Console.WindowWidth - 7 - section.Length, c), color);
            Console.WriteLine();
        }

        private static string FixedText(string s, int size)
        {
            if (s.Length > size)
                s = s.Substring(0, size - 3) + "...";

            if (s.Length < size)
                s = s.PadRight(size, ' ');

            return s;
        }

        private static void EraseLines(int lines)
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

        private static void WriteLine(string a, Color c1, bool wait = false)
        {
            Write(a, "", c1, c1, wait);
            Console.WriteLine();
        }

        private static void Write(string a, Color c1, bool wait = false) { Write(a, "", c1, c1, wait); }

        private static void WriteLine(string a, string b, Color c1, Color c2, bool wait = false)
        {
            Write(a, b, c1, c2, wait);
            Console.WriteLine();
        }

        private static void Write(string a, string b, Color c1, Color c2, bool wait = false)
        {
            foreach (char c in a)
            {
                Console.Write(c, c1);
            }

            foreach (char c in b)
            {
                Console.Write(c, c2);
                Thread.Sleep(animationSpeed);
            }

            if (wait)
                Thread.Sleep(waitSpeed);
        }

        private static void DrawProgressBar(int complete,
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
                decimal perc = complete / (decimal) (maxVal - 1);
                int chars = (int) Math.Floor(perc / (1 / (decimal) barSize));
                string p1 = string.Empty;
                string p2 = string.Empty;

                p1 = p1.PadLeft(chars, progressCharacter);
                p2 = p2.PadLeft(Math.Max(barSize - chars, 0), ' ');

                Console.Write(prefix + " ", dim);
                Console.Write(prefixColored + " ", chars >= barSize ? dim : info);

                if (showPbefore)
                    Console.Write($" {perc * 100:##0}%", chars >= barSize ? dim : attn);

                Console.Write("[", dim);
                Console.Write(p1, chars >= barSize ? dim : success);
                Console.Write(p2, dim);
                Console.Write("]", dim);

                if (!showPbefore)
                    Console.Write($" {perc * 100:##0}%", chars >= barSize ? dim : attn);

                //System.Console.ResetColor();
                System.Console.CursorLeft = left;
            }
            catch (Exception) { }
        }
    }
}