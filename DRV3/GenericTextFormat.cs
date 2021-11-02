using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DRV3
{
    public class GenericTextFormat
    {
        /// <summary>
        /// Translated sentences.
        /// </summary>
        protected List<string> sentences;
        protected List<uint> num; // Pointer number
        protected string originalSTX = string.Empty;
        protected string STXFFIleName = string.Empty;

        public GenericTextFormat()
        {

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
                using (FileStream NEWOutFile = new FileStream(Path.Combine(RepackFolder, STXFFIleName), FileMode.Create, FileAccess.Write))
                using (BinaryWriter OutFileBW = new BinaryWriter(NEWOutFile), TextUnicode = new BinaryWriter(NEWOutFile, Encoding.Unicode))
                {
                    OutFileBW.Write(header);

                    long pointZone = NEWOutFile.Position;
                    long[] sentencesOffeset = new long[sentences.Count];

                    // Fill the pointers zone with zeroes. I'll populate this at the end.
                    for (int i = 0; i < sentences.Count; i++)
                    {
                        OutFileBW.Write((long)0);
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
                            sentencesOffeset[i] = NEWOutFile.Position;

                            // Write the sentence n# [i] in the repacked file.
                            TextUnicode.Write(sentences[i].ToCharArray());

                            // Write down the null string terminator.
                            OutFileBW.Write((ushort)0x00);
                        }
                    }

                    // Now populate the pointers zone.
                    NEWOutFile.Seek(pointZone, SeekOrigin.Begin);

                    for (int i = 0; i < sentences.Count; i++)
                    {
                        if (num.Count == 0 || i >= num.Count)
                        {
                            // Workaround, this should never happen unless
                            // you manually edit the files in the "EXTRACTED FILES" folder
                            // and accidently add a line or more
                            OutFileBW.Write((uint)i);
                            Console.WriteLine("Something is wrong about this file (wrong number of lines?): " + STXFFIleName.Replace(".stx", ".txt"));
                        }
                        else
                        {
                            OutFileBW.Write((uint)num[i]);
                        }

                        OutFileBW.Write((uint)sentencesOffeset[i]);
                    }
                }
            }
            else
            {
                Console.WriteLine("Original STX file for " + STXFFIleName.Replace(".stx", ".txt") + " not found!");
            }
        }
    }
}
