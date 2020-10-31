using System.IO;

namespace DRV3
{
    public static class MagicID
    {
        public const uint STX = 0x54585453;


        public static bool Check(string fileToCheck, uint magicID)
        {
            using (FileStream f = new FileStream(fileToCheck, FileMode.Open, FileAccess.Read))
            using (BinaryReader rf = new BinaryReader(f))
            {
                if (rf.ReadUInt32() == magicID)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
