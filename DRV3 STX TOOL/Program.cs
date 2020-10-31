using System;

namespace CLI
{
    internal class Program
    {
        private static void Main()
        {
            Console.CursorVisible = true;
            Console.ResetColor();

            ASCII_Interface consoleInterface = new ASCII_Interface();

            consoleInterface.PrintFullInterface(ConsoleKey.DownArrow);

            while (true)
            {
                ConsoleKey keyPressedByUser = ReadInput.WaitForArrowKeys();
                consoleInterface.PrintFullInterface(keyPressedByUser);
            }
        }
    }
}
