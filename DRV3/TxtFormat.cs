using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Yarhl.IO;
using Yarhl.FileFormat;
using System.Linq;

namespace DRV3
{
    class TxtFormat : GenericTextFormat
    {
        public TxtFormat(string TxtAddress, string STX_Folder)
        {
            // Try to see if there is a .stx in the same directory where the .txt is
            STXFFIleName = Path.Combine(Path.GetFileNameWithoutExtension(TxtAddress) + ".stx");

            // Try to find out the path of the same .stx,
            // but in the STX folder instead (if it exists)
            if (File.Exists(Path.Combine(STX_Folder, STXFFIleName)))
            {
                originalSTX = Path.Combine(STX_Folder, STXFFIleName);
            }

            (sentences, num) = ExtractTextFromTxt(TxtAddress);
        }

        /// <summary>
        /// Read the all text from a file ".txt".
        /// </summary>
        /// <param name="TxtAddress">Txt absolute position.</param>
        /// <returns></returns>
        private (List<string>, List<uint>) ExtractTextFromTxt(string TxtAddress)
        {
            List<string> translatedSentences = new List<string>();
            List<string> redSentences = new List<string>();

            foreach (string line in File.ReadAllLines(TxtAddress))
            {
                redSentences.Add(line);
            }

            uint Index = 0;
            List<uint> num = new List<uint>();

            // We need to read the original STX to know the pointer numbers.
            // This isn't needed in most cases, but in some edge cases it is
            // necessary to read the original file, since ".txt" files don't have
            // a "line number" like ".po" files do.
            bool couldReadOriginalSTX = false;

            if (File.Exists(originalSTX))
            {
                // We don't care about the WRD folder
                STX mystx = new STX(originalSTX, "");
                num = mystx.GetNumENG().ToList();
                if(num.Count > 0)
                {
                    couldReadOriginalSTX = true;
                }
            }

            foreach (string entry in redSentences)
            {
                string sentence;

                if(entry == "{" && Index == 0)
                {
                    continue;
                }

                if(entry == "}" && Index == redSentences.Count - 1)
                {
                    continue;
                }

                if(!couldReadOriginalSTX)
                {
                    // This is a workaround and it should work in most cases
                    // If possible, use .po files instead

                    // Post-increment, so it stores the value of Index before incrementing it
                    num.Add(Index++);
                }

                if (entry == "[EMPTY_LINE]")
                {
                    sentence = "";
                }
                else if (entry.Trim() != null && entry.Trim() != "")
                {
                    sentence = entry.Replace("\\n", "\n");
                }
                else
                {
                    // TODO: Improve error handling
                    sentence = "[EMPTY_LINE]";
                }

                translatedSentences.Add(sentence); //And finally save the sentence.
            }

            return (translatedSentences, num); // Return all the sentences.
        }
    }
}
