using System.Collections.Generic;
using System.IO;
using System.Text;
using Yarhl.FileFormat;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace DRV3
{
    internal class PoFormat
    {
        /// <summary>
        /// Translated sentences.
        /// </summary>
        private readonly List<string> sentences;
        private readonly List<uint> num;
        private readonly string originalSTX = string.Empty;
        private readonly string STXFFIleName = string.Empty;

        public PoFormat(string PoAddress, string STX_Folder)
        {
            (sentences, num) = ExtracTextFromPo(PoAddress);

            STXFFIleName = Path.Combine(Path.GetFileNameWithoutExtension(PoAddress) + ".stx");

            if (File.Exists(Path.Combine(STX_Folder, STXFFIleName)))
            {
                originalSTX = Path.Combine(STX_Folder, STXFFIleName);
            }
        }

        /// <summary>
        /// Read the all text from a file ".po".
        /// </summary>
        /// <param name="PoAddress">Po absulute position.</param>
        /// <returns></returns>
        private (List<string>, List<uint>) ExtracTextFromPo(string PoAddress)
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

        /// <summary>
        /// Create the translated STX.
        /// </summary>
        /// <param name="RepackFolder">Folder where the new STX file are going to be placed.</param>
        public void BuildSTX(string RepackFolder)
        {
            if (originalSTX != string.Empty && File.Exists(originalSTX))
            {
                byte[] header;

                // Read and save the header from the original STX file.
                using (FileStream oF = new FileStream(originalSTX, FileMode.Open, FileAccess.Read))
                using (BinaryReader STXBBR = new BinaryReader(oF))
                {
                    STXBBR.ReadUInt64();
                    STXBBR.ReadUInt32();
                    uint headerSize = STXBBR.ReadUInt32();

                    header = new byte[headerSize];

                    oF.Seek(0, SeekOrigin.Begin);

                    oF.Read(header, 0, header.Length);
                }

                // Create the new STX file. 
                using (FileStream NEWPo = new FileStream(Path.Combine(RepackFolder, STXFFIleName), FileMode.Create, FileAccess.Write))
                using (BinaryWriter PoBW = new BinaryWriter(NEWPo), TextUnicode = new BinaryWriter(NEWPo, Encoding.Unicode))
                {
                    PoBW.Write(header);

                    long pointZone = NEWPo.Position;
                    long[] sentencesOffeset = new long[sentences.Count];

                    // Fill the pointers zone with zeroes. I'll populate this at the end.
                    for (int i = 0; i < sentences.Count; i++)
                    {
                        PoBW.Write((long)0);
                    }

                    for (int i = 0; i < sentences.Count; i++)
                    {
                        bool duplicate = false;

                        int x = 0;

                        // Check if the sentences "i" is a duplicate. This way we can save some space and write the sentence just once instead of multiples times.
                        while (x < i)
                        {
                            if (sentences[i] == sentences[x])
                            {
                                duplicate = true;
                                sentencesOffeset[i] = sentencesOffeset[x];
                                break;
                            }

                            x++;
                        }

                        if (duplicate == false)
                        {
                            sentencesOffeset[i] = NEWPo.Position;

                            // Write the sentence n# [i] in the repacked file.
                            TextUnicode.Write(sentences[i].ToCharArray());

                            // Write down the null string terminator.
                            PoBW.Write((ushort)0x00);
                        }
                    }

                    // Now populate the pointers zone.
                    NEWPo.Seek(pointZone, SeekOrigin.Begin);

                    for (int i = 0; i < sentences.Count; i++)
                    {
                        PoBW.Write((uint)num[i]);
                        PoBW.Write((uint)sentencesOffeset[i]);
                    }
                }
            }
        }
    }
}
