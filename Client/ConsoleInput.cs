namespace Client
{
    using System;

    static class ConsoleExtensions
    {
        public static string GetNonEmptyString(string prompt)
        {
            var input = string.Empty;
            while (string.IsNullOrEmpty(input))
            {
                Console.Write(prompt);
                input = Console.ReadLine();
            }
            return input;
        }

        public static void ErrorAndClose(string err, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(err, args);
            Console.ResetColor();
            Console.ReadLine();

            Environment.Exit(0);
        }
    }
}