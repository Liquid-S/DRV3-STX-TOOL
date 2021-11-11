using System.IO;

namespace DRV3
{
    public static class MagicID
    {
        public const uint STX = 0x54585453; // Converts to TXTS, which is STXT inverted

        public static bool Check(string fileToCheck, uint magicID)
        {
            using (FileStream f = new FileStream(fileToCheck, FileMode.Open, FileAccess.Read))
            using (BinaryReader rf = new BinaryReader(f))
            {
                // ReadUInt32 reads in low endian so it expects TXTS
                if (rf.ReadUInt32() == magicID)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
