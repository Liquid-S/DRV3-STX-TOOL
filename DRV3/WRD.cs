// Credits to https://github.com/jpmac26 for explain me how DRV3's files work.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DRV3
{
    public class WRD
    {
        public Dictionary<uint, string> charaNames = new Dictionary<uint, string>();

        public WRD(string fileWRD)
        {
            charaNames = ReadSpeakersFromWRD(fileWRD);
        }

        private Dictionary<uint, string> ReadSpeakersFromWRD(string fileWRD)
        {
            using (FileStream fs = new FileStream(fileWRD, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                ushort str_count = br.ReadUInt16();
                ushort label_count = br.ReadUInt16();
                ushort param_count = br.ReadUInt16();
                ushort sublabel_count = br.ReadUInt16();

                br.ReadUInt32(); // Padding???

                uint sublabel_offsets_ptr = br.ReadUInt32();
                uint label_offsets_ptr = br.ReadUInt32(); //opCodesZoneEnd
                uint label_names_ptr = br.ReadUInt32();
                uint params_ptr = br.ReadUInt32();
                uint str_ptr = br.ReadUInt32();

                string[] paramsList = new string[param_count];

                // Read Params
                fs.Seek(params_ptr, SeekOrigin.Begin);

                for (int i = 0; i < param_count; i++)
                {
                    byte[] sentence = new byte[br.ReadByte()];
                    fs.Read(sentence, 0, sentence.Length);

                    paramsList[i] = Encoding.Default.GetString(sentence);

                    br.ReadByte(); // = 0x0
                }

                // Read OP Codes
                fs.Seek(0x20, SeekOrigin.Begin);

                uint speakerCode = 0;

                while ((fs.Position != fs.Length) && (fs.Position < label_offsets_ptr))
                {
                    if (br.ReadByte() != 0x70)
                    {
                        continue;
                    }

                    byte tempVar = br.ReadByte();

                    // This is an atrocity, I'm well aware, but I did it with future features in mind. 
                    switch (tempVar)
                    {
                        case 0:     // FLG - Set Flag
                            br.ReadUInt32();
                            break;
                        case 0x01:  // IFF  -   If Flag
                        case 0x02:  // WAK  -   Wake? Work? (Seems to be used to configure game engine parameters)
                        case 0x03:  // IWAK -   If WAK
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x04:  // SWI  -   Begin switch statement
                            br.ReadUInt16();
                            break;
                        case 0x05:  // CAS  -   Switch Case
                            br.ReadUInt16();
                            break;
                        case 0x06:  // MPF  -   Map Flag?
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x07:  //SPW
                            break;
                        case 0x08:  // MOD  -   Set Modifier (Also used to configure game engine parameters)
                            br.ReadUInt64();
                            break;
                        case 0x09:  // HUM  -   Human? Seems to be used to initialize "interactable" objects in a map?
                        case 0x0A:  // CHK  -   Check?
                            br.ReadUInt16();
                            break;
                        case 0x0B:  // KTD  -   Kotodama?
                            br.ReadUInt32();
                            break;
                        case 0x0C:  // CLR  -   Clear?
                        case 0x0D:  // RET  -   break? There's another command later which is definitely break, though...
                            break;
                        case 0x0E:  // KNM  -   Kinematics (camera movement)
                            br.ReadUInt64();
                            br.ReadUInt16();
                            break;
                        case 0x0F:  // CAP  -   Camera Parameters?
                            break;
                        case 0x10:  // FIL  -   Load Script File & jump to label
                            br.ReadUInt32();
                            break;
                        case 0x11:  // END  -   End of script or switch case
                            break;
                        case 0x12:  // SUB  -   Jump to subroutine
                            br.ReadUInt32();
                            break;
                        case 0x13:  // RTN  -   break (called inside subroutine)
                            break;
                        case 0x14:  // LAB  -   Label number
                        case 0x15:  // JMP  -   Jump to label
                            br.ReadUInt16();
                            break;
                        case 0x16:  // MOV  -   Movie
                            br.ReadUInt32();
                            break;
                        case 0x17:  // FLS  -   Flash
                            br.ReadUInt64();
                            break;
                        case 0x18:  // FLM  -   Flash Modifier?
                            br.ReadUInt64();
                            br.ReadUInt32();
                            break;
                        case 0x19:  // VOI  -   Play voice clip
                            br.ReadUInt32();
                            break;
                        case 0x1A:  // BGM  -   Play BGM
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x1B:  // SE_  -   Play sound effect
                        case 0x1C:  // JIN  -   Play jingle
                            br.ReadUInt32();
                            break;
                        case 0x1D:  // CHN  -   Set active character ID (current person speaking)
                            byte[] temp1 = BitConverter.GetBytes(br.ReadInt16());
                            Array.Reverse(temp1);
                            speakerCode = BitConverter.ToUInt16(temp1, 0);
                            break;
                        case 0x1E:  // VIB  -   Camera Vibration
                        case 0x1F:  // FDS  -   Fade Screen
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x20:  // FLA  -   Camera Vibration
                            break;
                        case 0x21:  // LIG  -   Lighting Parameters
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x22:  // CHR  -   Character Parameters
                            br.ReadInt16();
                            byte[] temp3 = BitConverter.GetBytes(br.ReadInt16());
                            Array.Reverse(temp3);
                            speakerCode = BitConverter.ToUInt16(temp3, 0);
                            break;
                        case 0x23:  // BGD  -   Background Parameters
                            br.ReadUInt64();
                            break;
                        case 0x24:  // CUT  -   Cutin (display image for things like Truth Bullets, etc.)
                            br.ReadUInt32();
                            break;
                        case 0x25:  // ADF  -   Character Vibration?
                            br.ReadUInt64();
                            br.ReadUInt16();
                            break;
                        case 0x26:  // PAL  -   ???
                            break;
                        case 0x27:  // MAP  -   Load Map
                        case 0x28:  // OBJ  -   Load Object
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x29:  // BUL  - ???
                            br.ReadUInt64();
                            br.ReadUInt64();
                            break;
                        case 0x2A:  // CRF  -   Cross Fade
                            br.ReadUInt64();
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x2B:  // CAM  -   Camera command
                            br.ReadUInt64();
                            br.ReadUInt16();
                            break;
                        case 0x2C:  // KWM  -   Game/UI Mode
                            br.ReadUInt16();
                            break;
                        case 0x2D:  // ARE  -   ???
                            br.ReadInt32();
                            br.ReadInt16();
                            break;
                        case 0x2E:  // KEY  -   Enable/disable "key" items for unlocking areas
                            br.ReadInt32();
                            break;
                        case 0x2F:  // WIN  -   Window parameters
                            br.ReadInt64();
                            break;
                        case 0x30:  // MSC  -   ???
                        case 0x31:  // CSM  -   ???
                            break;
                        case 0x32:  // PST  -   Post-Processing
                        case 0x33:  // KNS  -   Kinematics Numeric parameters?
                            br.ReadInt64();
                            br.ReadInt16();
                            break;
                        case 0x34:  // FON  -   Set Font
                            br.ReadInt32();
                            break;
                        case 0x35:  // BGO  -   Load Background Object
                            br.ReadInt64();
                            br.ReadInt16();
                            break;
                        case 0x36:  // LOG  -   Edit Text Backlog?
                            break;
                        case 0x37:  // SPT  -   Used only in Class Trial? Always set to "non"?
                            br.ReadInt16();
                            break;
                        case 0x38:  // CDV  -   ???
                            br.ReadUInt64();
                            br.ReadUInt64();
                            br.ReadUInt32();
                            break;
                        case 0x39:  // SZM  -   Size Modifier (Class Trial)?
                            br.ReadUInt64();
                            break;
                        case 0x3A:  // PVI  -   Class Trial Chapter? Pre-trial intermission?
                            br.ReadUInt16();
                            break;
                        case 0x3B:  // EXP  -   Give EXP
                        case 0x3C:  // MTA  -   Used only in Class Trial? Usually set to "non"?
                            br.ReadUInt16();
                            break;
                        case 0x3D:  // MVP  -   Move object to its designated position?
                            br.ReadUInt32();
                            br.ReadUInt16();
                            break;
                        case 0x3E:  // POS  -   Object/Exisal position
                            br.ReadUInt64();
                            br.ReadUInt16();
                            break;
                        case 0x3F:  // ICO  -   Display a Program World character portrait
                            br.ReadUInt64();
                            break;
                        case 0x40:  // EAI  -   Exisal AI
                            br.ReadUInt64();
                            br.ReadUInt64();
                            br.ReadUInt32();
                            break;
                        case 0x41:  // COL  -   Set object collision
                            br.ReadInt32();
                            br.ReadInt16();
                            break;
                        case 0x42:  // CFP  -   Camera Follow Path? Seems to make the camera move in some way
                            br.ReadInt64();
                            br.ReadInt64();
                            br.ReadInt16();
                            break;
                        case 0x43:  // CLT  -   Text modifier command
                            br.ReadInt16();
                            break;
                        case 0x44:  // R=   -   ???
                            break;
                        case 0x45:  // PAD= -   Gamepad button symbol
                            break;
                        case 0x46:  // LOC= -   Display text string
                            byte[] temp2 = BitConverter.GetBytes(br.ReadInt16());
                            Array.Reverse(temp2);
                            charaNames.Add(BitConverter.ToUInt16(temp2, 0), paramsList[speakerCode]);
                            break;
                        case 0x47:  // BTN  -   Wait for button press
                        case 0x48:  // ENT  -   ???
                        case 0x49:  // CED  -   Check End (Used after IFF and IFW commands)
                            break;
                        case 0x4A:  // LBN  -   Local Branch Number (for branching case statements)
                            br.ReadInt16();
                            break;
                        case 0x4B:  // JMN  -   Jump to Local Branch (for branching case statements)
                            br.ReadInt16();
                            break;
                    }

                    //case 0x19:  // CHN  -   Set active character ID (current person speaking)
                    //    byte[] temp3 = BitConverter.GetBytes(br.ReadInt16());
                    //    Array.Reverse(temp3);

                    //    speakerCode = BitConverter.ToUInt16(temp3, 0);

                    //    br.ReadInt16();
                    //    break;
                }
            }

            return charaNames;
        }
    }
}
