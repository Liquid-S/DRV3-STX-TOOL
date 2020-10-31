// Credits to https://github.com/jpmac26 for explain me how DRV3's files work.

using System.IO;

namespace DRV3
{
    public static class Main
    {

        public static void ExtractTextFromSTXfiles(string STXFolder, string WRDFolder)
        {
            foreach (string STXfile in (Directory.GetFiles(STXFolder, "*.stx", SearchOption.TopDirectoryOnly)))
            {
                if (MagicID.Check(STXfile, MagicID.STX))
                {
                    STX STXobject = new STX(STXfile, WRDFolder);
                    STXobject.ConvertToPo("EXTRACTED FILES");
                }
            }
        }

        public static void RepackText(string poFolder, string STX_Folder)
        {
            string RepackFolder = "REPACKED FILES";

            if (Directory.Exists(RepackFolder))
            {
                Directory.Delete(RepackFolder, true);
                while (Directory.Exists(RepackFolder)) { }

                Directory.CreateDirectory(RepackFolder);
            }
            else
            {
                Directory.CreateDirectory(RepackFolder);               
            }

            foreach (string poFile in Directory.GetFiles(poFolder, "*.po", SearchOption.TopDirectoryOnly))
            {
                PoFormat pF = new PoFormat(poFile, STX_Folder);
                pF.BuildSTX(RepackFolder);
            }
        }
    }
}

