using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.FileFormat;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace DRV3
{
    internal class PoFormat : GenericTextFormat
    {

        public PoFormat(string PoAddress, string STX_Folder)
        {
            (sentences, num) = ExtractTextFromPo(PoAddress);

            // Try to see if there is a .stx in the same directory where the .po is
            STXFFIleName = Path.Combine(Path.GetFileNameWithoutExtension(PoAddress) + ".stx");

            // Try to find out the path of the same .stx,
            // but in the STX folder instead (if it exists)
            if (File.Exists(Path.Combine(STX_Folder, STXFFIleName)))
            {
                originalSTX = Path.Combine(STX_Folder, STXFFIleName);
            }
        }

        /// <summary>
        /// Read the all text from a file ".po".
        /// </summary>
        /// <param name="PoAddress">Po absolute position.</param>
        /// <returns></returns>
        private (List<string>, List<uint>) ExtractTextFromPo(string PoAddress)
        {
            List<string> translatedSentences = new List<string>();

            Po translatedPo = null;
            using (DataStream ds = new DataStream(PoAddress, FileOpenMode.Read))
            using (BinaryFormat binaryname = new BinaryFormat(ds))
            {
                translatedPo = binaryname.ConvertWith<Po2Binary, BinaryFormat, Po>();
            }

            List<uint> num = new List<uint>();

            foreach (PoEntry entry in translatedPo.Entries)
            {
                string sentence;

                num.Add(uint.Parse( string.Concat(entry.Context.Substring(0,4)))); 

                if (entry.Original == "[EMPTY_LINE]")
                {
                    sentence = "";
                }
                else if (entry.Translated.Trim() != null && entry.Translated.Trim() != "")
                {
                    sentence = entry.Translated;
                }
                else
                {
                    sentence = entry.Original;
                }
				
                translatedSentences.Add(sentence); //And finally save the sentence.
            }

            return (translatedSentences, num); // Return all the sentences.
        }
    }
}
