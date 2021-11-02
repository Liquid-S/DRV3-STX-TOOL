// Credits to https://github.com/jpmac26 for explain me how DRV3's files work.

using System.IO;

namespace DRV3
{
    public static class Main
    {
        public static bool UseTxtInsteadOfPo = false;

        public static void ExtractTextFromSTXfiles(string STXFolder, string WRDFolder)
        {
            string outFolder = "EXTRACTED FILES";

            // Iterate all the .stx files in the STXFolder directory (without searching in the subdirectories)
            foreach (string STXfile in (Directory.GetFiles(STXFolder, "*.stx", SearchOption.TopDirectoryOnly)))
            {
                if (MagicID.Check(STXfile, MagicID.STX))
                {
                    STX STXobject = new STX(STXfile, WRDFolder);
                    if(UseTxtInsteadOfPo)
                    {
                        STXobject.ConvertToTxt(outFolder);
                    } else
                    {
                        STXobject.ConvertToPo(outFolder);
                    }
                }
            }
        }

        public static void RepackText(string outFileFolder, string STX_Folder)
        {
            string RepackFolder = "REPACKED FILES";

            if (Directory.Exists(RepackFolder))
            {
                Directory.Delete(RepackFolder, true);
                while (Directory.Exists(RepackFolder)) { }
            }
            Directory.CreateDirectory(RepackFolder);

            // Iterate all the files in the STXFolder directory (without searching in the subdirectories)

            if(UseTxtInsteadOfPo)
            {
                foreach (string txtFile in Directory.GetFiles(outFileFolder, "*.txt", SearchOption.TopDirectoryOnly))
                {
                    TxtFormat tF = new TxtFormat(txtFile, STX_Folder);
                    tF.BuildSTX(RepackFolder);
                }
            }
            else
            {
                foreach (string poFile in Directory.GetFiles(outFileFolder, "*.po", SearchOption.TopDirectoryOnly))
                {
                    PoFormat pF = new PoFormat(poFile, STX_Folder);
                    pF.BuildSTX(RepackFolder);
                }
            }
        }
    }
}

