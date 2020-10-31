using CLI.InputOutput;
using System;
using System.IO;

namespace CLI
{
    public static class ReadInput
    {
        /// <summary>
        /// Ask to the user to insert the absolute path for the "fileType"'s folder.
        /// </summary>
        /// <param name="fileType">The files' extension you are asking to the user to search.</param>
        /// <returns></returns>
        public static string WaitForPath(string fileType)
        {
            string folderPath;
            Console.CursorVisible = true;

            do
            {
                ShowMessages.EventMessage($"Drop or write the absolute path for the {fileType}'s folder.\nE.g. \"C:\\MyGames\\DRV3\\STX_FOLDER\" without inverted commas.");
                folderPath = Console.ReadLine().Replace("\"", null);

                if (!Directory.Exists(folderPath))
                {
                    ShowMessages.ErrorMessage("Folder not found!");
                }
                else
                {
                    ShowMessages.EventMessage("Folder found!\n");
                }
            }
            while (!Directory.Exists(folderPath));

            Console.CursorVisible = false;

            return folderPath;
        }

        /// <summary>
        /// Wait for the user to input UpArrow, DownArrow or Enter.
        /// </summary>
        /// <returns>Returns the pressed key.</returns>
        public static ConsoleKey WaitForArrowKeys()
        {
            ConsoleKey k = Console.ReadKey(true).Key;

            while (k != ConsoleKey.UpArrow && k != ConsoleKey.DownArrow && k != ConsoleKey.Enter)
            {
                k = Console.ReadKey(true).Key;
            }

            return k;
        }
    }
}
