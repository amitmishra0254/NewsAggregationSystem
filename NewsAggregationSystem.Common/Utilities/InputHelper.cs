namespace NewsAggregationSystem.Common.Utilities
{
    public static class InputHelper
    {
        public static string ReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static string ReadPassword(string prompt)
        {
            Console.Write(prompt);
            string password = string.Empty;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }

            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        public static int ReadInt(string prompt)
        {
            int result;
            string input;
            do
            {
                Console.Write(prompt);
                input = Console.ReadLine();

                if (!int.TryParse(input, out result))
                {
                    Console.WriteLine("Invalid input. Please enter a valid integer.");
                }

            } while (!int.TryParse(input, out result));

            return result;
        }

        public static void PressKeyToContinue(string prompt = "Press any key to continue...")
        {
            Console.Write(prompt);
            Console.ReadKey();
        }
    }
}
