using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AC_fqrand_solver
{
    internal sealed class TownGeneration
    {
        public uint __qrand_idum;
        private float __qrand_itemp;

        private readonly bgdata_item[] bg_items = LoadBGData();
        private readonly ushort[][] fg_data;

#if DEBUG
        private int fqrand_calls = 0;
#endif

        public unsafe float fqrand()
        {
            __qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F;
            var temp = (__qrand_idum >> 9) | 0x3F800000;
            __qrand_itemp = *(float*)&temp;
#if DEBUG
            fqrand_calls++;
#endif
            return __qrand_itemp - 1.0f;
        }

        public uint fqrand_raw()
        {
#if DEBUG
            fqrand_calls++;
#endif
            return (__qrand_idum = (__qrand_idum * 0x19660D) + 0x3C6EF35F);
        }

        public TownGeneration(uint initial_seed = 1)
        {
            __qrand_idum = initial_seed;

            using BinaryReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("AC_fqrand_solver.Resources.fgdata.bin"));

            /* Load & sort items */
            fg_data = new ushort[0x1A0][];

            for (int i = 0; i < (int)reader.BaseStream.Length / 0x206; i++)
            {
                reader.BaseStream.Seek(i * 0x206, SeekOrigin.Begin);
                ushort fg_name = Swap16(reader.ReadUInt16()); // index
                ushort[] data = new ushort[16 * 16]; // items
                for (int x = 0; x < 16 * 16; x++)
                {
                    data[x] = Swap16(reader.ReadUInt16());
                }

                fg_data[fg_name] = data;
            }
        }

        private enum CliffSide : ushort
        {
            Up = 0,
            Down = 1,
            Any = 2
        }

        private enum RiverSide : ushort
        {
            Left = 0,
            Right = 1,
            Any = 2
        }

        private struct data_combi
        {
            public ushort bg_name;
            public ushort fg_name;
            public byte type; // Unsure about this name.
            public byte Padding;
        };

        private const int data_combi_table_number = 0x170; // This is only valid for Animal Crossing. Animal Forest+ has one less, and Animal Forest e+ has one more.

        private static readonly data_combi[] data_combi_table = new data_combi[368]
        {
            new data_combi { bg_name = 0x0124, fg_name = 0x00CB, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x003C, fg_name = 0x0000, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x00CB, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0004, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0005, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x000C, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0009, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x000B, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00F4, fg_name = 0x000D, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x000E, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x00CB, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x000F, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0010, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0011, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0012, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0013, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0014, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0015, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00F2, fg_name = 0x0016, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0017, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x001B, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00F6, fg_name = 0x001A, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x001B, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0069, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00CB, fg_name = 0x004E, type = 0x33, Padding = 0x00 },
            new data_combi { bg_name = 0x00F8, fg_name = 0x0022, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0027, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0033, fg_name = 0x002D, type = 0x1A, Padding = 0x00 },
            new data_combi { bg_name = 0x00DA, fg_name = 0x002E, type = 0x0D, Padding = 0x00 },
            new data_combi { bg_name = 0x010D, fg_name = 0x0001, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x010E, fg_name = 0x0001, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x010F, fg_name = 0x0001, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0110, fg_name = 0x0001, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0006, fg_name = 0x0029, type = 0x16, Padding = 0x00 },
            new data_combi { bg_name = 0x000F, fg_name = 0x002A, type = 0x36, Padding = 0x00 },
            new data_combi { bg_name = 0x0016, fg_name = 0x002B, type = 0x17, Padding = 0x00 },
            new data_combi { bg_name = 0x0018, fg_name = 0x002C, type = 0x1E, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x002F, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x005B, fg_name = 0x0030, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x0001, fg_name = 0x0031, type = 0x0F, Padding = 0x00 },
            new data_combi { bg_name = 0x0009, fg_name = 0x0032, type = 0x1D, Padding = 0x00 },
            new data_combi { bg_name = 0x000C, fg_name = 0x0033, type = 0x22, Padding = 0x00 },
            new data_combi { bg_name = 0x0013, fg_name = 0x0034, type = 0x10, Padding = 0x00 },
            new data_combi { bg_name = 0x001D, fg_name = 0x0035, type = 0x11, Padding = 0x00 },
            new data_combi { bg_name = 0x0020, fg_name = 0x0036, type = 0x18, Padding = 0x00 },
            new data_combi { bg_name = 0x0025, fg_name = 0x0037, type = 0x12, Padding = 0x00 },
            new data_combi { bg_name = 0x0028, fg_name = 0x0038, type = 0x19, Padding = 0x00 },
            new data_combi { bg_name = 0x002A, fg_name = 0x0039, type = 0x20, Padding = 0x00 },
            new data_combi { bg_name = 0x0030, fg_name = 0x003A, type = 0x13, Padding = 0x00 },
            new data_combi { bg_name = 0x0035, fg_name = 0x003B, type = 0x21, Padding = 0x00 },
            new data_combi { bg_name = 0x0037, fg_name = 0x003C, type = 0x24, Padding = 0x00 },
            new data_combi { bg_name = 0x003B, fg_name = 0x003D, type = 0x14, Padding = 0x00 },
            new data_combi { bg_name = 0x003E, fg_name = 0x003E, type = 0x1B, Padding = 0x00 },
            new data_combi { bg_name = 0x0042, fg_name = 0x003F, type = 0x15, Padding = 0x00 },
            new data_combi { bg_name = 0x00AA, fg_name = 0x0040, type = 0x28, Padding = 0x00 },
            new data_combi { bg_name = 0x00B2, fg_name = 0x0041, type = 0x29, Padding = 0x00 },
            new data_combi { bg_name = 0x00BA, fg_name = 0x0042, type = 0x2A, Padding = 0x00 },
            new data_combi { bg_name = 0x00C2, fg_name = 0x0043, type = 0x2B, Padding = 0x00 },
            new data_combi { bg_name = 0x00C8, fg_name = 0x0044, type = 0x2C, Padding = 0x00 },
            new data_combi { bg_name = 0x00CE, fg_name = 0x0045, type = 0x2D, Padding = 0x00 },
            new data_combi { bg_name = 0x00D4, fg_name = 0x0046, type = 0x2E, Padding = 0x00 },
            new data_combi { bg_name = 0x0022, fg_name = 0x0047, type = 0x1F, Padding = 0x00 },
            new data_combi { bg_name = 0x0047, fg_name = 0x0048, type = 0x26, Padding = 0x00 },
            new data_combi { bg_name = 0x0040, fg_name = 0x0049, type = 0x25, Padding = 0x00 },
            new data_combi { bg_name = 0x00AE, fg_name = 0x004A, type = 0x2F, Padding = 0x00 },
            new data_combi { bg_name = 0x00B6, fg_name = 0x004B, type = 0x30, Padding = 0x00 },
            new data_combi { bg_name = 0x00BE, fg_name = 0x004C, type = 0x31, Padding = 0x00 },
            new data_combi { bg_name = 0x00C5, fg_name = 0x004D, type = 0x32, Padding = 0x00 },
            new data_combi { bg_name = 0x00D1, fg_name = 0x004F, type = 0x34, Padding = 0x00 },
            new data_combi { bg_name = 0x00D7, fg_name = 0x0050, type = 0x35, Padding = 0x00 },
            new data_combi { bg_name = 0x00DB, fg_name = 0x0051, type = 0x0C, Padding = 0x00 },
            new data_combi { bg_name = 0x0010, fg_name = 0x0052, type = 0x36, Padding = 0x00 },
            new data_combi { bg_name = 0x001A, fg_name = 0x0053, type = 0x37, Padding = 0x00 },
            new data_combi { bg_name = 0x0024, fg_name = 0x0054, type = 0x38, Padding = 0x00 },
            new data_combi { bg_name = 0x002E, fg_name = 0x0055, type = 0x39, Padding = 0x00 },
            new data_combi { bg_name = 0x0039, fg_name = 0x0056, type = 0x3A, Padding = 0x00 },
            new data_combi { bg_name = 0x0041, fg_name = 0x0057, type = 0x3B, Padding = 0x00 },
            new data_combi { bg_name = 0x0049, fg_name = 0x0058, type = 0x3C, Padding = 0x00 },
            new data_combi { bg_name = 0x0045, fg_name = 0x005A, type = 0x1C, Padding = 0x00 },
            new data_combi { bg_name = 0x002C, fg_name = 0x0059, type = 0x23, Padding = 0x00 },
            new data_combi { bg_name = 0x0115, fg_name = 0x0028, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x010B, fg_name = 0x00C8, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0105, fg_name = 0x00C9, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0105, fg_name = 0x00CA, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0006, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00EF, fg_name = 0x00CC, type = 0x0B, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x00CD, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0002, fg_name = 0x00C0, type = 0x0F, Padding = 0x00 },
            new data_combi { bg_name = 0x0003, fg_name = 0x006B, type = 0x0F, Padding = 0x00 },
            new data_combi { bg_name = 0x0004, fg_name = 0x006C, type = 0x0F, Padding = 0x00 },
            new data_combi { bg_name = 0x0005, fg_name = 0x00BF, type = 0x0F, Padding = 0x00 },
            new data_combi { bg_name = 0x0014, fg_name = 0x0072, type = 0x10, Padding = 0x00 },
            new data_combi { bg_name = 0x00AB, fg_name = 0x0090, type = 0x28, Padding = 0x00 },
            new data_combi { bg_name = 0x00AC, fg_name = 0x0091, type = 0x28, Padding = 0x00 },
            new data_combi { bg_name = 0x00C3, fg_name = 0x009E, type = 0x2B, Padding = 0x00 },
            new data_combi { bg_name = 0x00C9, fg_name = 0x00A1, type = 0x2C, Padding = 0x00 },
            new data_combi { bg_name = 0x00D5, fg_name = 0x00A7, type = 0x2E, Padding = 0x00 },
            new data_combi { bg_name = 0x00D6, fg_name = 0x00A8, type = 0x2E, Padding = 0x00 },
            new data_combi { bg_name = 0x001B, fg_name = 0x0074, type = 0x37, Padding = 0x00 },
            new data_combi { bg_name = 0x0011, fg_name = 0x0071, type = 0x36, Padding = 0x00 },
            new data_combi { bg_name = 0x002F, fg_name = 0x007D, type = 0x39, Padding = 0x00 },
            new data_combi { bg_name = 0x003A, fg_name = 0x0081, type = 0x3A, Padding = 0x00 },
            new data_combi { bg_name = 0x003F, fg_name = 0x0084, type = 0x1B, Padding = 0x00 },
            new data_combi { bg_name = 0x0021, fg_name = 0x0077, type = 0x18, Padding = 0x00 },
            new data_combi { bg_name = 0x001E, fg_name = 0x0075, type = 0x11, Padding = 0x00 },
            new data_combi { bg_name = 0x0044, fg_name = 0x0086, type = 0x15, Padding = 0x00 },
            new data_combi { bg_name = 0x000A, fg_name = 0x006D, type = 0x1D, Padding = 0x00 },
            new data_combi { bg_name = 0x000D, fg_name = 0x006F, type = 0x22, Padding = 0x00 },
            new data_combi { bg_name = 0x0026, fg_name = 0x0078, type = 0x12, Padding = 0x00 },
            new data_combi { bg_name = 0x0031, fg_name = 0x007E, type = 0x13, Padding = 0x00 },
            new data_combi { bg_name = 0x003C, fg_name = 0x0082, type = 0x14, Padding = 0x00 },
            new data_combi { bg_name = 0x00B3, fg_name = 0x0095, type = 0x29, Padding = 0x00 },
            new data_combi { bg_name = 0x00BB, fg_name = 0x0099, type = 0x2A, Padding = 0x00 },
            new data_combi { bg_name = 0x0029, fg_name = 0x007A, type = 0x19, Padding = 0x00 },
            new data_combi { bg_name = 0x002B, fg_name = 0x007B, type = 0x20, Padding = 0x00 },
            new data_combi { bg_name = 0x0046, fg_name = 0x0087, type = 0x1C, Padding = 0x00 },
            new data_combi { bg_name = 0x00CF, fg_name = 0x00A4, type = 0x2D, Padding = 0x00 },
            new data_combi { bg_name = 0x0036, fg_name = 0x00B2, type = 0x21, Padding = 0x00 },
            new data_combi { bg_name = 0x002D, fg_name = 0x007C, type = 0x23, Padding = 0x00 },
            new data_combi { bg_name = 0x00B4, fg_name = 0x0096, type = 0x29, Padding = 0x00 },
            new data_combi { bg_name = 0x00BC, fg_name = 0x009A, type = 0x2A, Padding = 0x00 },
            new data_combi { bg_name = 0x0038, fg_name = 0x0080, type = 0x24, Padding = 0x00 },
            new data_combi { bg_name = 0x0032, fg_name = 0x007F, type = 0x13, Padding = 0x00 },
            new data_combi { bg_name = 0x0027, fg_name = 0x0079, type = 0x12, Padding = 0x00 },
            new data_combi { bg_name = 0x001F, fg_name = 0x0076, type = 0x11, Padding = 0x00 },
            new data_combi { bg_name = 0x0015, fg_name = 0x0073, type = 0x10, Padding = 0x00 },
            new data_combi { bg_name = 0x010C, fg_name = 0x00CE, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00B1, fg_name = 0x00C1, type = 0x45, Padding = 0x00 },
            new data_combi { bg_name = 0x0007, fg_name = 0x005B, type = 0x16, Padding = 0x00 },
            new data_combi { bg_name = 0x0008, fg_name = 0x005C, type = 0x16, Padding = 0x00 },
            new data_combi { bg_name = 0x000B, fg_name = 0x006E, type = 0x1D, Padding = 0x00 },
            new data_combi { bg_name = 0x000E, fg_name = 0x0070, type = 0x22, Padding = 0x00 },
            new data_combi { bg_name = 0x0017, fg_name = 0x005E, type = 0x17, Padding = 0x00 },
            new data_combi { bg_name = 0x0034, fg_name = 0x005D, type = 0x1A, Padding = 0x00 },
            new data_combi { bg_name = 0x003D, fg_name = 0x0083, type = 0x14, Padding = 0x00 },
            new data_combi { bg_name = 0x0043, fg_name = 0x0085, type = 0x15, Padding = 0x00 },
            new data_combi { bg_name = 0x00AD, fg_name = 0x0092, type = 0x28, Padding = 0x00 },
            new data_combi { bg_name = 0x00B5, fg_name = 0x00B3, type = 0x29, Padding = 0x00 },
            new data_combi { bg_name = 0x00BD, fg_name = 0x009B, type = 0x2A, Padding = 0x00 },
            new data_combi { bg_name = 0x00C4, fg_name = 0x009F, type = 0x2B, Padding = 0x00 },
            new data_combi { bg_name = 0x00CA, fg_name = 0x00A2, type = 0x2C, Padding = 0x00 },
            new data_combi { bg_name = 0x00D0, fg_name = 0x00A5, type = 0x2D, Padding = 0x00 },
            new data_combi { bg_name = 0x00E4, fg_name = 0x0067, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E4, fg_name = 0x0066, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0068, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0019, fg_name = 0x005F, type = 0x1E, Padding = 0x00 },
            new data_combi { bg_name = 0x0023, fg_name = 0x0060, type = 0x1F, Padding = 0x00 },
            new data_combi { bg_name = 0x00AF, fg_name = 0x0093, type = 0x2F, Padding = 0x00 },
            new data_combi { bg_name = 0x00B7, fg_name = 0x0097, type = 0x30, Padding = 0x00 },
            new data_combi { bg_name = 0x00C6, fg_name = 0x00A0, type = 0x32, Padding = 0x00 },
            new data_combi { bg_name = 0x00BF, fg_name = 0x009C, type = 0x31, Padding = 0x00 },
            new data_combi { bg_name = 0x00CC, fg_name = 0x00A3, type = 0x33, Padding = 0x00 },
            new data_combi { bg_name = 0x00D2, fg_name = 0x00A6, type = 0x34, Padding = 0x00 },
            new data_combi { bg_name = 0x00D8, fg_name = 0x00A9, type = 0x35, Padding = 0x00 },
            new data_combi { bg_name = 0x00B0, fg_name = 0x0094, type = 0x2F, Padding = 0x00 },
            new data_combi { bg_name = 0x00B8, fg_name = 0x0098, type = 0x30, Padding = 0x00 },
            new data_combi { bg_name = 0x00C0, fg_name = 0x009D, type = 0x31, Padding = 0x00 },
            new data_combi { bg_name = 0x005C, fg_name = 0x0088, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x005D, fg_name = 0x0089, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x005E, fg_name = 0x008A, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x005F, fg_name = 0x008B, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x0060, fg_name = 0x008C, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x0061, fg_name = 0x008D, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x0062, fg_name = 0x008E, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x0063, fg_name = 0x008F, type = 0x27, Padding = 0x00 },
            new data_combi { bg_name = 0x00DC, fg_name = 0x00AA, type = 0x0C, Padding = 0x00 },
            new data_combi { bg_name = 0x00DD, fg_name = 0x00B6, type = 0x0C, Padding = 0x00 },
            new data_combi { bg_name = 0x00DE, fg_name = 0x00B7, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00DF, fg_name = 0x00AB, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E0, fg_name = 0x00AE, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E1, fg_name = 0x00B8, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E2, fg_name = 0x00AF, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E3, fg_name = 0x00B9, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E4, fg_name = 0x00AD, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00E8, fg_name = 0x00B0, type = 0x0D, Padding = 0x00 },
            new data_combi { bg_name = 0x00E9, fg_name = 0x00BA, type = 0x0D, Padding = 0x00 },
            new data_combi { bg_name = 0x00EA, fg_name = 0x00BB, type = 0x0D, Padding = 0x00 },
            new data_combi { bg_name = 0x00EB, fg_name = 0x00B1, type = 0x0D, Padding = 0x00 },
            new data_combi { bg_name = 0x00B9, fg_name = 0x00B4, type = 0x46, Padding = 0x00 },
            new data_combi { bg_name = 0x00C1, fg_name = 0x00C2, type = 0x47, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x006A, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0061, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0062, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0063, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0064, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00AA, fg_name = 0x0065, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00D3, fg_name = 0x00B5, type = 0x4A, Padding = 0x00 },
            new data_combi { bg_name = 0x00D9, fg_name = 0x00AC, type = 0x4B, Padding = 0x00 },
            new data_combi { bg_name = 0x00F0, fg_name = 0x00BC, type = 0x0B, Padding = 0x00 },
            new data_combi { bg_name = 0x00F1, fg_name = 0x00BD, type = 0x0B, Padding = 0x00 },
            new data_combi { bg_name = 0x00C7, fg_name = 0x00C3, type = 0x48, Padding = 0x00 },
            new data_combi { bg_name = 0x00CD, fg_name = 0x00BE, type = 0x49, Padding = 0x00 },
            new data_combi { bg_name = 0x00FF, fg_name = 0x00CF, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x010A, fg_name = 0x00D0, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0103, fg_name = 0x0016, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0104, fg_name = 0x0016, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0106, fg_name = 0x00CB, type = 0x4C, Padding = 0x00 },
            new data_combi { bg_name = 0x0107, fg_name = 0x00CB, type = 0x4D, Padding = 0x00 },
            new data_combi { bg_name = 0x0108, fg_name = 0x00CB, type = 0x4E, Padding = 0x00 },
            new data_combi { bg_name = 0x0109, fg_name = 0x00CB, type = 0x4F, Padding = 0x00 },
            new data_combi { bg_name = 0x0012, fg_name = 0x00E8, type = 0x36, Padding = 0x00 },
            new data_combi { bg_name = 0x004C, fg_name = 0x00CB, type = 0x00, Padding = 0x00 },
            new data_combi { bg_name = 0x004D, fg_name = 0x00CB, type = 0x01, Padding = 0x00 },
            new data_combi { bg_name = 0x004E, fg_name = 0x00CB, type = 0x02, Padding = 0x00 },
            new data_combi { bg_name = 0x004F, fg_name = 0x00CB, type = 0x3D, Padding = 0x00 },
            new data_combi { bg_name = 0x0052, fg_name = 0x00CB, type = 0x09, Padding = 0x00 },
            new data_combi { bg_name = 0x0053, fg_name = 0x00CB, type = 0x04, Padding = 0x00 },
            new data_combi { bg_name = 0x0054, fg_name = 0x00CB, type = 0x3E, Padding = 0x00 },
            new data_combi { bg_name = 0x0057, fg_name = 0x00CB, type = 0x0A, Padding = 0x00 },
            new data_combi { bg_name = 0x0058, fg_name = 0x00CB, type = 0x05, Padding = 0x00 },
            new data_combi { bg_name = 0x0059, fg_name = 0x00CB, type = 0x08, Padding = 0x00 },
            new data_combi { bg_name = 0x0064, fg_name = 0x0068, type = 0x44, Padding = 0x00 },
            new data_combi { bg_name = 0x0065, fg_name = 0x00DC, type = 0x44, Padding = 0x00 },
            new data_combi { bg_name = 0x0066, fg_name = 0x00E4, type = 0x44, Padding = 0x00 },
            new data_combi { bg_name = 0x0067, fg_name = 0x0069, type = 0x0E, Padding = 0x00 },
            new data_combi { bg_name = 0x0068, fg_name = 0x00E2, type = 0x0E, Padding = 0x00 },
            new data_combi { bg_name = 0x0069, fg_name = 0x00E3, type = 0x0E, Padding = 0x00 },
            new data_combi { bg_name = 0x006D, fg_name = 0x006A, type = 0x42, Padding = 0x00 },
            new data_combi { bg_name = 0x006E, fg_name = 0x00E0, type = 0x42, Padding = 0x00 },
            new data_combi { bg_name = 0x006F, fg_name = 0x00E1, type = 0x42, Padding = 0x00 },
            new data_combi { bg_name = 0x00E5, fg_name = 0x00DD, type = 0x43, Padding = 0x00 },
            new data_combi { bg_name = 0x00EC, fg_name = 0x00E5, type = 0x41, Padding = 0x00 },
            new data_combi { bg_name = 0x00ED, fg_name = 0x00E6, type = 0x41, Padding = 0x00 },
            new data_combi { bg_name = 0x00EE, fg_name = 0x00E7, type = 0x41, Padding = 0x00 },
            new data_combi { bg_name = 0x00E6, fg_name = 0x00DE, type = 0x43, Padding = 0x00 },
            new data_combi { bg_name = 0x00E7, fg_name = 0x00DF, type = 0x43, Padding = 0x00 },
            new data_combi { bg_name = 0x005D, fg_name = 0x0002, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x0023, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0100, fg_name = 0x0025, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0101, fg_name = 0x0026, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0102, fg_name = 0x0024, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0000, fg_name = 0x00D1, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0078, fg_name = 0x0061, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x0079, fg_name = 0x0062, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x0078, fg_name = 0x0061, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0079, fg_name = 0x0062, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0082, fg_name = 0x0065, type = 0x40, Padding = 0x00 },
            new data_combi { bg_name = 0x0050, fg_name = 0x00CB, type = 0x50, Padding = 0x00 },
            new data_combi { bg_name = 0x0055, fg_name = 0x00CB, type = 0x51, Padding = 0x00 },
            new data_combi { bg_name = 0x00F9, fg_name = 0x0151, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0083, fg_name = 0x0152, type = 0x40, Padding = 0x00 },
            new data_combi { bg_name = 0x0084, fg_name = 0x0153, type = 0x40, Padding = 0x00 },
            new data_combi { bg_name = 0x0085, fg_name = 0x0154, type = 0x40, Padding = 0x00 },
            new data_combi { bg_name = 0x0086, fg_name = 0x0155, type = 0x40, Padding = 0x00 },
            new data_combi { bg_name = 0x0087, fg_name = 0x0156, type = 0x52, Padding = 0x00 },
            new data_combi { bg_name = 0x0088, fg_name = 0x0157, type = 0x52, Padding = 0x00 },
            new data_combi { bg_name = 0x0089, fg_name = 0x0158, type = 0x52, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x007A, fg_name = 0x0063, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x007B, fg_name = 0x0064, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x007C, fg_name = 0x0159, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x007D, fg_name = 0x015A, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x007E, fg_name = 0x015B, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x007F, fg_name = 0x015C, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x0080, fg_name = 0x015D, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x0081, fg_name = 0x015E, type = 0x3F, Padding = 0x00 },
            new data_combi { bg_name = 0x001C, fg_name = 0x0160, type = 0x37, Padding = 0x00 },
            new data_combi { bg_name = 0x0048, fg_name = 0x0162, type = 0x26, Padding = 0x00 },
            new data_combi { bg_name = 0x004A, fg_name = 0x015F, type = 0x3C, Padding = 0x00 },
            new data_combi { bg_name = 0x004B, fg_name = 0x0161, type = 0x3C, Padding = 0x00 },
            new data_combi { bg_name = 0x00B1, fg_name = 0x0007, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x004B, fg_name = 0x0011, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0174, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0013, fg_name = 0x0175, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0030, fg_name = 0x0176, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0088, fg_name = 0x0177, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00CE, fg_name = 0x0178, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00BA, fg_name = 0x0179, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x000F, fg_name = 0x017A, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x007A, fg_name = 0x017B, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00AE, fg_name = 0x017C, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0030, fg_name = 0x017D, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0042, fg_name = 0x017E, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0008, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0116, fg_name = 0x017F, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0118, fg_name = 0x0180, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0117, fg_name = 0x0181, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x011A, fg_name = 0x0182, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0119, fg_name = 0x0183, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0184, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x005A, fg_name = 0x0185, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0006, fg_name = 0x0020, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x00AB, fg_name = 0x0021, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0122, fg_name = 0x0186, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x006A, fg_name = 0x0187, type = 0x54, Padding = 0x00 },
            new data_combi { bg_name = 0x006B, fg_name = 0x0188, type = 0x54, Padding = 0x00 },
            new data_combi { bg_name = 0x006C, fg_name = 0x0189, type = 0x54, Padding = 0x00 },
            new data_combi { bg_name = 0x008A, fg_name = 0x018A, type = 0x55, Padding = 0x00 },
            new data_combi { bg_name = 0x008B, fg_name = 0x018B, type = 0x55, Padding = 0x00 },
            new data_combi { bg_name = 0x008C, fg_name = 0x018C, type = 0x55, Padding = 0x00 },
            new data_combi { bg_name = 0x008D, fg_name = 0x018D, type = 0x64, Padding = 0x00 },
            new data_combi { bg_name = 0x0090, fg_name = 0x00CB, type = 0x5E, Padding = 0x00 },
            new data_combi { bg_name = 0x0070, fg_name = 0x018F, type = 0x63, Padding = 0x00 },
            new data_combi { bg_name = 0x0074, fg_name = 0x018E, type = 0x62, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x00FE, fg_name = 0x00CB, type = 0x53, Padding = 0x00 },
            new data_combi { bg_name = 0x0090, fg_name = 0x00CB, type = 0x5E, Padding = 0x00 },
            new data_combi { bg_name = 0x0090, fg_name = 0x00CB, type = 0x5E, Padding = 0x00 },
            new data_combi { bg_name = 0x0091, fg_name = 0x00CB, type = 0x5F, Padding = 0x00 },
            new data_combi { bg_name = 0x0091, fg_name = 0x00CB, type = 0x5F, Padding = 0x00 },
            new data_combi { bg_name = 0x0091, fg_name = 0x00CB, type = 0x5F, Padding = 0x00 },
            new data_combi { bg_name = 0x0091, fg_name = 0x00CB, type = 0x5F, Padding = 0x00 },
            new data_combi { bg_name = 0x0092, fg_name = 0x00CB, type = 0x60, Padding = 0x00 },
            new data_combi { bg_name = 0x0092, fg_name = 0x00CB, type = 0x60, Padding = 0x00 },
            new data_combi { bg_name = 0x0092, fg_name = 0x00CB, type = 0x60, Padding = 0x00 },
            new data_combi { bg_name = 0x0093, fg_name = 0x00CB, type = 0x61, Padding = 0x00 },
            new data_combi { bg_name = 0x0093, fg_name = 0x00CB, type = 0x61, Padding = 0x00 },
            new data_combi { bg_name = 0x0093, fg_name = 0x00CB, type = 0x61, Padding = 0x00 },
            new data_combi { bg_name = 0x0093, fg_name = 0x00CB, type = 0x61, Padding = 0x00 },
            new data_combi { bg_name = 0x0093, fg_name = 0x00CB, type = 0x61, Padding = 0x00 },
            new data_combi { bg_name = 0x0051, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0056, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0090, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0091, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0092, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0093, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0094, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0095, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0096, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0097, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0098, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x0099, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x009C, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x009D, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x009E, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x009F, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A0, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A1, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A2, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A3, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A4, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A5, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A6, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A7, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x009A, fg_name = 0x00CB, type = 0x66, Padding = 0x00 },
            new data_combi { bg_name = 0x009A, fg_name = 0x00CB, type = 0x66, Padding = 0x00 },
            new data_combi { bg_name = 0x009B, fg_name = 0x00CB, type = 0x67, Padding = 0x00 },
            new data_combi { bg_name = 0x009B, fg_name = 0x00CB, type = 0x67, Padding = 0x00 },
            new data_combi { bg_name = 0x009B, fg_name = 0x00CB, type = 0x67, Padding = 0x00 },
            new data_combi { bg_name = 0x009B, fg_name = 0x00CB, type = 0x67, Padding = 0x00 },
            new data_combi { bg_name = 0x0123, fg_name = 0x0190, type = 0xFF, Padding = 0x00 },
            new data_combi { bg_name = 0x0071, fg_name = 0x0192, type = 0x63, Padding = 0x00 },
            new data_combi { bg_name = 0x0075, fg_name = 0x0191, type = 0x62, Padding = 0x00 },
            new data_combi { bg_name = 0x0072, fg_name = 0x0194, type = 0x63, Padding = 0x00 },
            new data_combi { bg_name = 0x0076, fg_name = 0x0193, type = 0x62, Padding = 0x00 },
            new data_combi { bg_name = 0x0073, fg_name = 0x0196, type = 0x63, Padding = 0x00 },
            new data_combi { bg_name = 0x0077, fg_name = 0x0195, type = 0x62, Padding = 0x00 },
            new data_combi { bg_name = 0x008E, fg_name = 0x0197, type = 0x64, Padding = 0x00 },
            new data_combi { bg_name = 0x008F, fg_name = 0x0198, type = 0x64, Padding = 0x00 },
            new data_combi { bg_name = 0x00A8, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00A9, fg_name = 0x00CB, type = 0x68, Padding = 0x00 },
            new data_combi { bg_name = 0x00F3, fg_name = 0x0199, type = 0xFF, Padding = 0x00 }
        };

        private enum block_info
        {
            PLACE_NONE = (0 << 0),
            PLACE_PLAYER_HOUSE = (1 << 0),
            PLACE_SHOP = (1 << 1),
            PLACE_SQUARE = (1 << 2),
            PLACE_POLICE_BOX = (1 << 3),
            PLACE_POST_OFFICE = (1 << 4),
            PLACE_STATION = (1 << 5),
            PLACE_CLIFF = (1 << 6),
            PLACE_RIVER = (1 << 7),
            PLACE_FALL = (1 << 8),
            PLACE_BRIDGE = (1 << 9),
            PLACE_RAILROAD = (1 << 10),
            PLACE_MARIN = (1 << 11),
            PLACE_OUTLINE = (1 << 12),
            PLACE_TUNNEL = (1 << 13),
            PLACE_SLOPE = (1 << 14),
            PLACE_POOL = (1 << 15),
            PLACE_DUMP = (1 << 16),
            PLACE_MUSEUM = (1 << 17),
            PLACE_RIVER2 = (1 << 18),
            PLACE_TAILORS = (1 << 19),
            PLACE_OCEAN = (1 << 20),
            PLACE_ISLAND = (1 << 21),
            PLACE_OFFING = (1 << 22),
            PLACE_RIVER7 = (1 << 23),
            PLACE_CLIFF1 = (1 << 24),
            PLACE_CLIFF2 = (1 << 25),
            PLACE_CLIFF3 = (1 << 26),
            PLACE_CLIFF4 = (1 << 27),
            PLACE_CLIFF5 = (1 << 28),
            PLACE_CLIFF6 = (1 << 29),
            PLACE_DOCK = (1 << 30),
            PLACE_ISLAND_LEFT = (1 << 31) /* Island left acre */
        }

        private static readonly int[] mRF_block_info =
        {
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_RAILROAD | block_info.PLACE_TUNNEL),
            (int)(block_info.PLACE_RAILROAD | block_info.PLACE_TUNNEL),
            (int)(block_info.PLACE_STATION | block_info.PLACE_RAILROAD),
            (int)(block_info.PLACE_RAILROAD | block_info.PLACE_DUMP),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RAILROAD | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_PLAYER_HOUSE),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_FALL | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_NONE),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF3),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF4),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF5),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_RIVER2 | block_info.PLACE_CLIFF6),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF3),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF4),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF5),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF6),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_SLOPE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_MARIN),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_MARIN | block_info.PLACE_RIVER2 | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_SHOP | block_info.PLACE_RAILROAD),
            (int)(block_info.PLACE_SQUARE),
            (int)(block_info.PLACE_POST_OFFICE | block_info.PLACE_RAILROAD),
            (int)(block_info.PLACE_POLICE_BOX),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_CLIFF2),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_CLIFF3),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_CLIFF4),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_CLIFF5),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_POOL | block_info.PLACE_CLIFF6),
            (int)(block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_OUTLINE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_RAILROAD | block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OUTLINE),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_MARIN | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_NONE),
            (int)(block_info.PLACE_MUSEUM),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_TAILORS),
            (int)(block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_RAILROAD | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_RIVER7),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_ISLAND | block_info.PLACE_ISLAND_LEFT),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_ISLAND),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_DOCK),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_MARIN | block_info.PLACE_OCEAN | block_info.PLACE_OFFING),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
            (int)(block_info.PLACE_CLIFF | block_info.PLACE_RIVER | block_info.PLACE_BRIDGE | block_info.PLACE_CLIFF1),
        };

        // Generation Common Things
        private static readonly int[] blockGroup_428 = new int[16]
        {
            0x0F, 0x15, 0x28, 0x2E, 0x2F, 0x35, 0x36, 0x3C,
            0x16, 0x26, 0x16, 0x1C, 0x1D, 0x21, 0x22, 0x26
        };

        private static readonly int[] x_offset_409 = new int[4] { 0, -1, 0, 1 }; // Directions: North, West, South, East
        private static readonly int[] z_offset_410 = new int[4] { -1, 0, 1, 0 };

        private static readonly int[] system_block_info = new int[108]
        {
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000001,
            0x00000002, 0x00000004, 0x00000008, 0x00000010,
            0x00000020, 0x00000040, 0x00000001, 0x00000002,
            0x00000004, 0x00000008, 0x00000010, 0x00000020,
            0x00000040, 0x00000001, 0x00000002, 0x00000004,
            0x00000008, 0x00000010, 0x00000001, 0x00000008,
            0x00000010, 0x00000020, 0x00000040, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000001, 0x00000002,
            0x00000004, 0x00000008, 0x00000010, 0x00000020,
            0x00000040, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000001,
            0x00000004, 0x00000008, 0x00000008, 0x00000010,
            0x00000020, 0x00000040, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000000, 0x00000000, 0x00000000,
            0x00000000, 0x00000001, 0x00000008, 0x00000010
        };

        private static readonly ushort[] exceptional_table = new ushort[54]
        {
            0x0050, 0x0051, 0x0055, 0x0056, 0x0078, 0x0090, 0x0079, 0x0091,
            0x007A, 0x0092, 0x007B, 0x0093, 0x007C, 0x0094, 0x007D, 0x0095,
            0x007E, 0x0096, 0x007F, 0x0097, 0x0080, 0x0098, 0x0081, 0x0099,
            0x0082, 0x009C, 0x0083, 0x009D, 0x0084, 0x009E, 0x0085, 0x009F,
            0x0086, 0x00A0, 0x0087, 0x00A1, 0x0088, 0x00A2, 0x0089, 0x00A3,
            0x008A, 0x00A4, 0x008B, 0x00A5, 0x008C, 0x00A6, 0x008D, 0x00A7,
            0x008E, 0x00A8, 0x008F, 0x00A9, 0x0125, 0x0125
        };

        // Two Layered Town Base Layout
        private static readonly byte[] DefaultTownStructure = new byte[70]
        {
        //   00    01    02    03    04    05    06
            0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, // 0
            0x09, 0x0C, 0x0C, 0x0B, 0x0C, 0x0C, 0x0A, // A
            0x02, 0x27, 0x27, 0x0E, 0x27, 0x27, 0x04, // B
            0x02, 0x27, 0x27, 0x27, 0x27, 0x27, 0x04, // C
            0x02, 0x27, 0x27, 0x27, 0x27, 0x27, 0x04, // D
            0x02, 0x27, 0x27, 0x27, 0x27, 0x27, 0x04, // E
            0x50, 0x27, 0x27, 0x27, 0x27, 0x27, 0x51, // F
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65, // G
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66, // H
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67  // I
        };

        // Two Layered Town Variables

        #region Cliff Variables
        // Cliff Variables
        private static readonly byte[] cliff_startA_factor = new byte[2] { 0x0F, 0x13 };
        private static readonly byte[] cliff_startB_factor = new byte[3] { 0x0F, 0x10, 0x13 };
        private static readonly byte[] cliff_startC_factor = new byte[2] { 0x0F, 0x10 };

        private static readonly byte[][] cliff_start_table = new byte[4][]
        {
            cliff_startA_factor,
            cliff_startA_factor,
            cliff_startB_factor,
            cliff_startC_factor
        };

        private static readonly byte[] cliff1_next = new byte[3] { 0x0F, 0x10, 0x13 };
        private static readonly byte[] cliff2_next = new byte[2] { 0x11, 0x12 };
        private static readonly byte[] cliff3_next = new byte[2] { 0x11, 0x12 };
        private static readonly byte[] cliff4_next = new byte[3] { 0x0F, 0x10, 0x13 };
        private static readonly byte[] cliff5_next = new byte[2] { 0x14, 0x15 };
        private static readonly byte[] cliff6_next = new byte[2] { 0x14, 0x15 };
        private static readonly byte[] cliff7_next = new byte[3] { 0x0F, 0x10, 0x13 };

        private static readonly byte[][] cliff_next_data = new byte[7][]
        {
            cliff1_next, cliff2_next, cliff3_next, cliff4_next,
            cliff5_next, cliff6_next, cliff7_next
        };

        private static readonly int[] cliff_info = new int[7] { 1, 2, 4, 8, 0x10, 0x20, 0x40 };

        /// <summary>
        /// The Direction of the next cliff section (0 = North, 1 = East? 2 = South, 3 = West?)
        /// </summary>
        private static readonly byte[] cliff_next_direct = new byte[8] // I think this is only 7 bytes long
        {
            3, 0, 0, 3, 2, 2, 3, 0
        };

        #endregion

        #region River Variables
        // River Variables
        private static readonly byte[] river1_album_data = new byte[7] { 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C };
        private static readonly byte[] river2_album_data = new byte[7] { 0x1D, 0x1E, 0x1F, 0x20, 0x21, 0xFF, 0xFF };
        private static readonly byte[] river3_album_data = new byte[7] { 0x22, 0xFF, 0xFF, 0x23, 0x24, 0x25, 0x26 };
        private static readonly byte[] river_no_album_data = new byte[7] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        private static readonly byte[][] river_cliff_album_data = new byte[7][]
        {
            river1_album_data, river2_album_data, river3_album_data, river_no_album_data,
            river_no_album_data, river_no_album_data, river_no_album_data
        };

        /// <summary>
        /// Valid river start X-Acres
        /// </summary>
        private static readonly int[] startX_table = new int[4] { 1, 2, 4, 5 };

        /// <summary>
        /// River & Cliff Acres
        /// </summary>
        private static readonly byte[] cross_data = new byte[7] { 0x16, 0x17, 0x1A, 0x1E, 0x1F, 0x25, 0x26 };

        private static readonly byte[] river1_next = new byte[3] { 0x28, 0x2B, 0x2D };
        private static readonly byte[] river2_next = new byte[2] { 0x29, 0x2C };
        private static readonly byte[] river3_next = new byte[2] { 0x2A, 0x2E };
        private static readonly byte[] river4_next = new byte[2] { 0x29, 0x2C };
        private static readonly byte[] river5_next = new byte[3] { 0x28, 0x2B, 0x2D };
        private static readonly byte[] river6_next = new byte[2] { 0x2A, 0x2E };
        private static readonly byte[] river7_next = new byte[3] { 0x28, 0x2B, 0x2D };

        private static readonly byte[][] river_next_data = new byte[7][]
        {
            river1_next, river2_next, river3_next, river4_next,
            river5_next, river6_next, river7_next
        };

        /// <summary>
        /// The Direction of the next river section (0 = North, 1 = East? 2 = South, 3 = West?)
        /// </summary>
        private static readonly byte[] river_next_direct = new byte[7] { 2, 3, 1, 3, 2, 1, 2 };

        #endregion


        // Three Layered Town Layouts
        private static readonly byte[] step3_blocks3 = new byte[70]
        {
            0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x08,
            0x09, 0x0D, 0x0C, 0x0B, 0x0C, 0x0C, 0x0A,
            0x02, 0x2B, 0x2C, 0x0E, 0x12, 0x0F, 0x3E,
            0x3D, 0x0F, 0x16, 0x0F, 0x10, 0x12, 0x3E,
            0x3D, 0x0F, 0x1A, 0x27, 0x27, 0x11, 0x04,
            0x02, 0x27, 0x1C, 0x0F, 0x0F, 0x10, 0x04,
            0x50, 0x27, 0x28, 0x27, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocks7 = new byte[70]
        {
            0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x08,
            0x09, 0x0D, 0x0C, 0x0B, 0x0C, 0x0C, 0x0A,
            0x02, 0x2B, 0x2C, 0x0E, 0x27, 0x12, 0x3E,
            0x3D, 0x0F, 0x16, 0x0F, 0x0F, 0x10, 0x04,
            0x02, 0x27, 0x28, 0x12, 0x0F, 0x0F, 0x3E,
            0x3D, 0x0F, 0x16, 0x10, 0x27, 0x27, 0x04,
            0x50, 0x27, 0x28, 0x27, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocks7R = new byte[70]
        {
            0x05, 0x00, 0x00, 0x00, 0x00, 0x01, 0x08,
            0x09, 0x0C, 0x0C, 0x0B, 0x0C, 0x0D, 0x0A,
            0x3D, 0x13, 0x27, 0x0E, 0x2E, 0x2D, 0x04,
            0x02, 0x15, 0x0F, 0x0F, 0x16, 0x0F, 0x3E,
            0x3D, 0x0F, 0x0F, 0x13, 0x28, 0x27, 0x04,
            0x02, 0x27, 0x27, 0x15, 0x16, 0x0F, 0x3E,
            0x50, 0x27, 0x27, 0x27, 0x28, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocks8 = new byte[70]
        {
            0x05, 0x00, 0x00, 0x00, 0x00, 0x01, 0x08,
            0x09, 0x0C, 0x0C, 0x0B, 0x0C, 0x0D, 0x0A,
            0x3D, 0x0F, 0x13, 0x0E, 0x2E, 0x2D, 0x04,
            0x02, 0x27, 0x15, 0x0F, 0x1A, 0x12, 0x3E,
            0x3D, 0x0F, 0x0F, 0x13, 0x1C, 0x10, 0x04,
            0x02, 0x27, 0x27, 0x15, 0x16, 0x0F, 0x3E,
            0x50, 0x27, 0x27, 0x27, 0x28, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocksB = new byte[70]
        {
            0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x08,
            0x09, 0x0C, 0x0C, 0x0B, 0x0D, 0x0C, 0x0A,
            0x3D, 0x0F, 0x13, 0x0E, 0x28, 0x12, 0x3E,
            0x02, 0x27, 0x15, 0x0F, 0x16, 0x10, 0x04,
            0x3D, 0x0F, 0x13, 0x2E, 0x2D, 0x27, 0x04,
            0x02, 0x27, 0x15, 0x16, 0x0F, 0x0F, 0x3E,
            0x50, 0x27, 0x27, 0x28, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocksBR = new byte[70]
        {
            0x05, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08,
            0x09, 0x0C, 0x0D, 0x0B, 0x0C, 0x0C, 0x0A,
            0x3D, 0x13, 0x28, 0x0E, 0x12, 0x0F, 0x3E,
            0x02, 0x15, 0x16, 0x0F, 0x10, 0x27, 0x04,
            0x02, 0x27, 0x2B, 0x2C, 0x12, 0x0F, 0x3E,
            0x3D, 0x0F, 0x0F, 0x16, 0x10, 0x27, 0x04,
            0x50, 0x27, 0x27, 0x28, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocksE = new byte[70]
        {
            0x05, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08,
            0x09, 0x0C, 0x0D, 0x0B, 0x0C, 0x0C, 0x0A,
            0x3D, 0x13, 0x28, 0x0E, 0x12, 0x0F, 0x3E,
            0x02, 0x15, 0x16, 0x0F, 0x10, 0x27, 0x04,
            0x02, 0x27, 0x2B, 0x2C, 0x12, 0x0F, 0x3E,
            0x3D, 0x0F, 0x0F, 0x16, 0x10, 0x27, 0x04,
            0x50, 0x27, 0x27, 0x28, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocksER = new byte[70]
        {
            0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x08,
            0x09, 0x0C, 0x0C, 0x0B, 0x0D, 0x0C, 0x0A,
            0x3D, 0x0F, 0x13, 0x0E, 0x28, 0x12, 0x3E,
            0x02, 0x27, 0x15, 0x0F, 0x16, 0x10, 0x04,
            0x3D, 0x0F, 0x13, 0x2E, 0x2D, 0x27, 0x04,
            0x02, 0x27, 0x15, 0x16, 0x0F, 0x0F, 0x3E,
            0x50, 0x27, 0x27, 0x28, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocksF = new byte[70]
        {
            0x05, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08,
            0x09, 0x0C, 0x0D, 0x0B, 0x0C, 0x0C, 0x0A,
            0x02, 0x27, 0x28, 0x0E, 0x12, 0x0F, 0x3E,
            0x3D, 0x0F, 0x16, 0x0F, 0x10, 0x27, 0x04,
            0x02, 0x27, 0x2B, 0x29, 0x2C, 0x27, 0x04,
            0x3D, 0x0F, 0x0F, 0x0F, 0x16, 0x0F, 0x3E,
            0x50, 0x27, 0x27, 0x27, 0x28, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[] step3_blocksFR = new byte[70]
        {
            0x05, 0x00, 0x00, 0x00, 0x01, 0x00, 0x08,
            0x09, 0x0C, 0x0C, 0x0B, 0x0D, 0x0C, 0x0A,
            0x3D, 0x0F, 0x13, 0x0E, 0x28, 0x27, 0x04,
            0x02, 0x27, 0x15, 0x0F, 0x16, 0x0F, 0x3E,
            0x02, 0x27, 0x2E, 0x2A, 0x2D, 0x27, 0x04,
            0x3D, 0x0F, 0x16, 0x0F, 0x0F, 0x0F, 0x3E,
            0x50, 0x27, 0x28, 0x27, 0x27, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        // Unused 3-layer town layout found in DnM+. It was removed in the transition to Animal Crossing.
        private static readonly byte[] step3_blocksD = new byte[70]
        {
            0x05, 0x00, 0x00, 0x00, 0x00, 0x01, 0x08,
            0x09, 0x0c, 0x0c, 0x0b, 0x0c, 0x0d, 0x0a,
            0x3d, 0x0f, 0x13, 0x0e, 0x2e, 0x2d, 0x04,
            0x02, 0x27, 0x15, 0x0f, 0x16, 0x0f, 0x3e,
            0x3d, 0x0f, 0x0f, 0x0f, 0x1a, 0x27, 0x04,
            0x02, 0x27, 0x27, 0x27, 0x1c, 0x0f, 0x3e,
            0x50, 0x27, 0x27, 0x27, 0x28, 0x27, 0x51,
            0x65, 0x65, 0x65, 0x65, 0x65, 0x65, 0x65,
            0x53, 0x53, 0x53, 0x66, 0x62, 0x63, 0x66,
            0x53, 0x53, 0x53, 0x67, 0x67, 0x67, 0x67
        };

        private static readonly byte[][] step3_blockss = new byte[10][]
        {
            step3_blocks3, step3_blocks7, step3_blocks7R, step3_blocks8,
            step3_blocksB, step3_blocksBR, step3_blocksE, step3_blocksER,
            step3_blocksF, step3_blocksFR
        };

        private static readonly IReadOnlyDictionary<byte, ushort[]> TownAcrePool = new Dictionary<byte, ushort[]>
        {
            { 0x00, new ushort[] {0x0324} }, // Upper Border Cliff
            { 0x01, new ushort[] {0x0328} }, // Upper Border Cliff w/ River
            { 0x02, new ushort[] {0x032C} }, // Left Border Cliff
            // 0x03?
            { 0x04, new ushort[] {0x0338} }, // Right Border Cliff
            { 0x05, new ushort[] {0x0344} }, // Left Border Corner Cliff
            // 0x06?
            // 0x07?
            { 0x08, new ushort[] {0x0348} }, // Right Border Corner Cliff
            { 0x09, new ushort[] {0x0334} }, // Left Border Cliff w/ Tunnel
            { 0x0A, new ushort[] {0x0340} }, // Right Border Cliff w/ Tunnel
            { 0x0B, new ushort[] {0x0154, 0x02F0, 0x02F4} }, // Train Stations
            { 0x0C, new ushort[] {0x0118, 0x0294, 0x0298} }, // Dump Acres
            { 0x0D, new ushort[] {0x0070, 0x02B8, 0x02BC, 0x02C0, 0x02C4} }, // Rivers w/ Train Track
            { 0x0E, new ushort[] {0x0358, 0x035C, 0x0360} }, // Player House Acres
            { 0x0F, new ushort[] {0x009C, 0x015C, 0x0160, 0x0164, 0x0168} }, // Horizontal Cliffs
            { 0x10, new ushort[] {0x00A8, 0x016C, 0x01F4} }, // Left Corner Cliffs
            { 0x11, new ushort[] {0x00AC, 0x01A0, 0x01F0} }, // Left Side Cliffs
            { 0x12, new ushort[] {0x00B4, 0x01B0, 0x01EC} }, // Left Side Inverted Cliffs
            { 0x13, new ushort[] {0x00C0, 0x01B4, 0x01E8} }, // Right Side Inverted Cliffs
            { 0x14, new ushort[] {0x00CC, 0x01B8, 0x0218} }, // Right Side Cliffs
            { 0x15, new ushort[] {0x00D4, 0x01A4, 0x021C} }, // Right Side Corner Cliffs
            { 0x16, new ushort[] {0x0084, 0x0200, 0x0204} }, // South Flowing Waterfall w/ Horizontal Cliff
            { 0x17, new ushort[] {0x008C, 0x0210} }, // South Flowing Waterfall w/ Left Corner Cliff
            { 0x18, new ushort[] {0x00B0, 0x019C} }, // South Flowing River (Upper) w/ Left Cliff
            { 0x19, new ushort[] {0x00B8, 0x01C4} }, // South Flowing River (Upper) w/ Left Inverted Corner Cliff
            { 0x1A, new ushort[] {0x006C, 0x0214} }, // South Flowing Waterfall w/ Right Inverted Corner Cliff
            { 0x1B, new ushort[] {0x00D0, 0x0198} }, // South Flowing River (Lower) w/ Right Cliff
            { 0x1C, new ushort[] {0x0138, 0x01CC} }, // South Flowing River (Lower) w/ Right Corner Cliff
            { 0x1D, new ushort[] {0x00A0, 0x01A8, 0x0208} }, // East Flowing River (Upper) w/ Horizontal Cliff
            { 0x1E, new ushort[] {0x0090, 0x0244} }, // East Flowing Waterfall w/ Left Corner Cliff
            { 0x1F, new ushort[] {0x00F4, 0x0248} }, // East Flowing Waterfall w/ Left Cliff
            { 0x20, new ushort[] {0x00BC, 0x01C8} }, // East Flowing River (Upper) w/ Inverted Left Corner Cliff
            { 0x21, new ushort[] {0x00C4, 0x01D4} }, // East Flowing River (Upper) w/ Inverted Right Corner Cliff
            { 0x22, new ushort[] {0x00A4, 0x01AC, 0x020C} }, // West Flowing River (Upper) w/ Horizontal Cliff
            { 0x23, new ushort[] {0x013C, 0x01D8} }, // West Flowing River (Upper) w/ Inverted Left Corner Cliff
            { 0x24, new ushort[] {0x00C8, 0x01E4} }, // West Flowing River (Upper) w/ Inverted Right Corner Cliff
            { 0x25, new ushort[] {0x00FC} }, // West Flowing Waterfall w/ Right Cliff
            { 0x26, new ushort[] {0x00F8, 0x0414} }, // West Flowing Waterfall w/ Right Corner Cliff
            { 0x27, new ushort[] {0x0094, 0x0098, 0x0274, 0x0278, 0x027C, 0x0280, 0x0284, 0x0288, 0x028C, 0x0290} }, // Grass acres
            { 0x28, new ushort[] {0x00D8, 0x0170, 0x0174, 0x0220} }, // River south
            { 0x29, new ushort[] {0x00DC, 0x01BC, 0x01DC, 0x0224} }, // River east
            { 0x2A, new ushort[] {0x00E0, 0x01C0, 0x01E0, 0x0228} }, // River west
            { 0x2B, new ushort[] {0x00E4, 0x0178, 0x022C} }, // River south > east
            { 0x2C, new ushort[] {0x00E8, 0x017C, 0x0230} }, // River east > south
            { 0x2D, new ushort[] {0x00EC, 0x01D0, 0x0234} }, // River south > west
            { 0x2E, new ushort[] {0x00F0, 0x0180, 0x0184} }, // River west > south
            { 0x2F, new ushort[] {0x0100, 0x024C, 0x0268} }, // River south w/ bridge
            { 0x30, new ushort[] {0x0104, 0x0250, 0x026C} }, // River east w/ brdige
            { 0x31, new ushort[] {0x0108, 0x0258, 0x0270} }, // River west w/ bridge
            { 0x32, new ushort[] {0x010C, 0x0254} }, // River south > east w/ bridge
            { 0x33, new ushort[] {0x0060, 0x025C} }, // River east > south w/ bridge
            { 0x34, new ushort[] {0x0110, 0x0260} }, // River south > west w/ bridge
            { 0x35, new ushort[] {0x0114, 0x0264} }, // River west > south w/ bridge
            { 0x36, new ushort[] {0x0088, 0x011C, 0x018C, 0x0320} }, // Ramp south
            { 0x37, new ushort[] {0x0120, 0x0188, 0x0410} },
            { 0x38, new ushort[] {0x0124} },
            { 0x39, new ushort[] {0x0128, 0x0190} },
            { 0x3A, new ushort[] {0x012C, 0x0194} },
            { 0x3B, new ushort[] {0x0130} },
            { 0x3C, new ushort[] {0x0134, 0x0418, 0x041C} },
            { 0x3D, new ushort[] {0x0330} },
            { 0x3E, new ushort[] {0x033C} },
            { 0x3F, new ushort[] {0x03A0, 0x03A4, 0x03F0, 0x03F4, 0x03F8, 0x03FC, 0x0400, 0x0404, 0x0408, 0x040C} },
            { 0x40, new ushort[] {0x03B0, 0x03C0, 0x03C4, 0x03C8, 0x03CC} },
            { 0x41, new ushort[] {0x0374, 0x0378, 0x037C} }, // Nook's Acres
            { 0x42, new ushort[] {0x0364, 0x0368, 0x036C} }, // Wishing Well
            { 0x43, new ushort[] {0x0370, 0x0380, 0x0384} }, // Post Office Acres
            { 0x44, new ushort[] {0x034C, 0x0350, 0x0354} }, // Police Station
            { 0x45, new ushort[] {0x01FC} },
            { 0x46, new ushort[] {0x02C8} },
            { 0x47, new ushort[] {0x02CC} },
            { 0x48, new ushort[] {0x02F8} },
            { 0x49, new ushort[] {0x02FC} },
            { 0x4A, new ushort[] {0x02E8} },
            { 0x4B, new ushort[] {0x02EC} },
            { 0x4C, new ushort[] {0x0310} },
            { 0x4D, new ushort[] {0x0314} },
            { 0x4E, new ushort[] {0x0318} },
            { 0x4F, new ushort[] {0x031C} },
            { 0x50, new ushort[] {0x03B4} },
            { 0x51, new ushort[] {0x03B8} },
            { 0x52, new ushort[] {0x03D0, 0x03D4, 0x03D8} },
            { 0x53, new ushort[] {0x03DC, 0x03E0, 0x03E4, 0x03E8, 0x03EC, 0x04A8, 0x04AC, 0x04B0, 0x04B4, 0x04B8, 0x04BC, 0x04C0, 0x04C4, 0x04C8, 0x04CC, 0x04D0, 0x04D4, 0x04D8, 0x04DC} },
            { 0x54, new ushort[] {0x0480, 0x0484, 0x0488} }, // Museum
            { 0x55, new ushort[] {0x048C, 0x0490, 0x0494} },
            // 0x56?
            // 0x57?
            // 0x58?
            // 0x59?
            // 0x5A?
            // 0x5B?
            // 0x5C?
            // 0x5D?
            { 0x5E, new ushort[] {0x049C, 0x04E0, 0x04E4} },
            { 0x5F, new ushort[] {0x04E8, 0x04EC, 0x04F0, 0x04F4} },
            { 0x60, new ushort[] {0x04F8, 0x04FC, 0x0500} },
            { 0x61, new ushort[] {0x0504, 0x0508, 0x050C, 0x0510, 0x0514} },
            { 0x62, new ushort[] {0x04A4, 0x0598, 0x05A0, 0x05A8} },
            { 0x63, new ushort[] {0x04A0, 0x0594, 0x059C, 0x05A4} },
            { 0x64, new ushort[] {0x0498, 0x05AC, 0x05B0} },
            // 0x65? these are handled slighly differently by the game.
            { 0x66, new ushort[] {0x0578, 0x057C} },
            { 0x67, new ushort[] {0x0580, 0x0584, 0x0588, 0x058C} },
            { 0x68, new ushort[] {0x0518, 0x051C, 0x0520, 0x0524, 0x0528, 0x052C, 0x0530, 0x0534, 0x0538, 0x053C, 0x0540, 0x0544, 0x0548, 0x054C, 0x0550, 0x0554, 0x0558, 0x055C, 0x0560, 0x0564, 0x0568, 0x056C, 0x0570, 0x0574, 0x05B4, 0x05B8} },
            // 69 - 6B
        };

        private int D2toD1(int AcreX, int AcreY)
        {
            return AcreY * 7 + AcreX;
        }

        private void D1toD2(int Index, out int X, out int Y)
        {
            X = Index % 7;
            Y = Index / 7;
        }

        private int GetXYCoordinateForBlockType(byte[] Data, int BlockType, out int X, out int Y)
        {
            int Index = -1;
            X = Y = -1;

            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] == BlockType)
                {
                    Index = i;
                    X = i % 7;
                    Y = i / 7;
                }
            }

            return Index;
        }

        /// <summary>
        /// Returns a random integer between [0 - <param name="maxValue"/>).
        /// </summary>
        /// <param name="maxValue">The exclusive upper limit of the random number to be generated.</param>
        /// <param name="seed">The optional seed parameter used to geenrate the random number.</param>
        /// <returns></returns>
        private int GetRandom(int maxValue) => (int)(fqrand() * maxValue);

        /// <summary>
        /// Selects the "step" mode, or layer count, of your town. If 0, it's a 2 layered town. If 1, it's a 3 layered town.
        /// </summary>
        /// <returns>Step Mode</returns>
        private int GetRandomStepMode()
        {
            return GetRandom(100) < 15 ? 1 : 0;
        }

        /// <summary>
        /// Returns 0x1FF
        /// </summary>
        /// <returns>0x1FF</returns>
        private int MakePerfectBit()
        {
            int Perfect = 0;
            for (int i = 0; i < 9; i++)
                Perfect |= 1 << i;
            return Perfect;
        }

        /// <summary>
        /// Faster way of calculating MakePerfectBit
        /// </summary>
        const int PERFECT_BIT_CONSTEXPR = (1 << 9) - 1;

        /// <summary>
        /// Takes the current X & Y acre and a direction, and returns the next X & Y acre for that direction
        /// </summary>
        /// <param name="X">The next X acre</param>
        /// <param name="Y">The next Y acre</param>
        /// <param name="AcreX">The current X acre</param>
        /// <param name="AcreY">The current Y acre</param>
        /// <param name="Direction">The direction of the current feature</param>
        private void Direct2BlockNo(out int X, out int Y, int AcreX, int AcreY, int Direction)
        {
            X = AcreX + x_offset_409[Direction & 3];
            Y = AcreY + z_offset_410[Direction & 3];
        }

        private bool CheckBlockGroup(int BlockType, int AcreTypeSet)
        {
            if (AcreTypeSet != 8)
            {
                int MinType = blockGroup_428[AcreTypeSet * 2];
                int MaxType = blockGroup_428[AcreTypeSet * 2 + 1];

                return BlockType >= MinType && BlockType <= MaxType;
            }
            else
            {
                if (BlockType >= blockGroup_428[0] && BlockType <= blockGroup_428[1])
                {
                    return true;
                }
                else if (BlockType >= blockGroup_428[6] && BlockType <= blockGroup_428[7])
                {
                    return true;
                }
                else if (BlockType >= blockGroup_428[8] && BlockType <= blockGroup_428[9])
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Checks that both acre X & Y are within their valid parameters.
        /// </summary>
        /// <param name="AcreX">X-Acre</param>
        /// <param name="AcreY">Y-Acre</param>
        /// <param name="AcreXMin">Minimum X-Acre value</param>
        /// <param name="AcreXMax">Maximum X-Acre value</param>
        /// <param name="AcreYMin">Minimum Y-Acre value</param>
        /// <param name="AcreYMax">Maximum Y-Acre value</param>
        /// <returns>bool AcresAreValid</returns>
        private bool CheckCorrectBlockNo(int AcreX, int AcreY, int AcreXMin, int AcreXMax, int AcreYMin, int AcreYMax)
            => (AcreX >= AcreXMin && AcreX <= AcreXMax && AcreY >= AcreYMin && AcreY <= AcreYMax);

        private int GetSystemBlockInfo(int BlockType)
            => system_block_info[BlockType & 0xFF];

        // Cliff code
        private bool DecideBaseCliff(ref byte[] AcreData)
        {
            int CliffStartTableIndex = GetRandom(4);
            byte[] CliffStartTable = cliff_start_table[CliffStartTableIndex];
            byte CliffStartAcreType = CliffStartTable[GetRandom(CliffStartTable.Length)];

            // Set the first in-town cliff acre
            int CliffStartAcre = D2toD1(1, CliffStartTableIndex + 2);
            AcreData[CliffStartAcre] = CliffStartAcreType;

            // Set the border acre
            int CliffBorderStartAcre = D2toD1(0, CliffStartTableIndex + 2);
            AcreData[CliffBorderStartAcre] = 0x3D;

            // Trace Cliff
            TraceCliffBlock(ref AcreData, 1, CliffStartTableIndex + 2);

            // Set Cliff End Acre
            SetEndCliffBlock(ref AcreData);

            // Check Cliff is valid
            return LastCheckCliff(AcreData, 1, CliffStartTableIndex + 2);
        }

        private bool TraceCliffBlock(ref byte[] AcreData, int AcreX, int AcreY)
        {
            byte CurrentCliffAcreType = AcreData[D2toD1(AcreX, AcreY)];
            int TraceState = 0;
            byte CliffSubtractionValue = (byte)(CurrentCliffAcreType - 0xF);
            int CliffSubtractionValueShifted = CliffSubtractionValue << 2;
            int X = 0, Y = 0;

            while (TraceState == 0)
            {
                byte[] CliffNext = cliff_next_data[CliffSubtractionValue];
                byte CliffAcreType = CliffNext[GetRandom(CliffNext.Length)];
                byte CliffAdjustValue = cliff_next_direct[CliffSubtractionValue];

                Direct2BlockNo(out X, out Y, AcreX, AcreY, CliffAdjustValue);

                CliffAdjustValue = cliff_next_direct[(byte)(CliffAcreType - 0xF)];
                Direct2BlockNo(out int X2, out int Y2, X, Y, CliffAdjustValue);

                bool BlockCheck1 = CheckCorrectBlockNo(X, Y, 1, 5, 2, 5);
                bool BlockCheck2 = CheckCorrectBlockNo(X2, Y2, 1, 6, 2, 5);
                if (BlockCheck1 && BlockCheck2)
                {
                    int CliffReplaceAcre1 = D2toD1(X2, Y2);
                    if (AcreData[CliffReplaceAcre1] == 0xE)
                    {
                        //Console.WriteLine("\nHouse acre is the next acre! Cannot continue.");
                        return false;
                    }

                    int CliffReplaceAcre2 = D2toD1(X, Y);
                    if (AcreData[CliffReplaceAcre2] != 0x27)
                    {
                        //Console.WriteLine("\nCurrently selected acre isn't a grass block! Cannot continue.");
                        return false;
                    }

                    AcreData[CliffReplaceAcre2] = CliffAcreType;
                    if (X == 5)
                    {
                        TraceState = 2;
                    }
                    else
                    {
                        TraceState = 1;
                    }
                }
                else
                {
                    //Console.WriteLine(string.Format("\nBlock number was incorrect! Cannot continue. Block Values: X1: {2} Y1: {3} Check1: {0}\n X2: {4} Y2: {5} Check2: {1}", BlockCheck1.ToString(), BlockCheck2.ToString(), X, Y, X2, Y2));
                    return false;
                }
            }

            if (TraceState == 1)
            {
                return TraceCliffBlock(ref AcreData, X, Y);
            }
            else
            {
                return TraceState == 2;
            }
        }

        private void SetEndCliffBlock(ref byte[] AcreData) // This doesn't always work. Double check it at some point.
        {
            int AcreY = 0;
            int DirectType = 0;

            for (int Y = 0; Y < 6; Y++)
            {
                byte AcreType = AcreData[D2toD1(5, Y)];
                if (AcreType >= 0xF && AcreType <= 0x15)
                {
                    AcreY = Y;
                    DirectType = (byte)(AcreType - 0xF);
                }
            }

            byte CliffNextDirectValue = cliff_next_direct[DirectType];
            if (CliffNextDirectValue == 3) // Cliff is going west, end immediately
            {
                AcreData[D2toD1(6, AcreY)] = 0x3E;
            }
            else
            {
                byte[] CliffNextSet = cliff_next_data[DirectType];
                if (CliffNextSet.Length > 0)
                {
                    for (int i = 0; i < CliffNextSet.Length; i++)
                    {
                        byte CliffDirectValue = cliff_next_direct[CliffNextSet[i] - 0xF];
                        if (CliffDirectValue == 3)
                        {
                            Direct2BlockNo(out int X2, out int Y2, 5, AcreY, CliffNextDirectValue);
                            AcreData[D2toD1(X2, Y2)] = CliffNextSet[i];
                            AcreData[D2toD1(X2 + 1, Y2)] = 0x3E;
                        }
                    }
                }
            }
        }

        private bool LastCheckCliff(byte[] AcreData, int AcreX, int AcreY)
        {
            byte CliffAcreType = AcreData[D2toD1(AcreX, AcreY)];
            int Y = AcreY;
            while (CheckBlockGroup(CliffAcreType, 0) == true)
            {
                byte CliffDirectValue = cliff_next_direct[CliffAcreType - 0xF];
                Direct2BlockNo(out AcreX, out AcreY, AcreX, AcreY, CliffDirectValue);
                CliffAcreType = AcreData[D2toD1(AcreX, AcreY)];
            }

            return AcreX > 5 && AcreY != Y; // Might be AcreY >= Y or AcreY == Y
        }

        // River Generation Code

        /// <summary>
        /// Gets the direction of the next river section.
        /// </summary>
        /// <param name="RiverIdx">The current river block index</param>
        /// <returns>The direction of the next river section</returns>
        private int RiverIdx2NextDirect(int RiverIdx)
        {
            if (RiverIdx > -1 && RiverIdx < 7)
            {
                return river_next_direct[RiverIdx];
            }
            return 2; // Return south by default
        }

        private byte RiverAlbuminCliff(byte BlockType, byte AlbuminType)
        {
            if (CheckBlockGroup(BlockType, 0) && CheckBlockGroup(AlbuminType, 1))
            {
                sbyte AdjustType = (sbyte)(BlockType - 0xF);
                sbyte AlbuminAdjustType = (sbyte)(AlbuminType - 0x28);

                if (AlbuminAdjustType < 7 && AdjustType > -1 && AdjustType < 7)
                {
                    return river_cliff_album_data[AlbuminAdjustType][AdjustType];
                }
            }
            return 0xFF;
        }

        /// <summary>
        /// Checks if the river crosses the centerpoint of the map.
        /// </summary>
        /// <param name="AcreData">The current acre data</param>
        /// <returns>int NumberOfTimesCrossed</returns>
        private int GetCenterCrossZoneRiverCount(byte[] AcreData)
        {
            int AcreY = 2;
            int CrossZoneCount = 0;
            while (AcreY <= 5)
            {
                if (CheckBlockGroup(AcreData[D2toD1(3, AcreY)], 1) == true)
                {
                    CrossZoneCount++;
                }
                AcreY++;
            }
            return CrossZoneCount;
        }

        private bool TraceRiverPart1(ref byte[] AcreData, out int AcreX, out int AcreY)
        {
            int RiverTraceState = 0;
            AcreX = 0;
            AcreY = 0;
            while (RiverTraceState == 0)
            {
                int RiverXStartAcre = startX_table[GetRandom(4)];
                byte[] RiverStartData = river_next_data[0];
                byte RiverStartType = RiverStartData[GetRandom(RiverStartData.Length)];

                Direct2BlockNo(out int X, out int Y, RiverXStartAcre, 1, 2);
                int RiverAbsoluteStartAcre = D2toD1(X, Y);
                int NextRiverSectionDirection = RiverIdx2NextDirect((byte)(RiverStartType - 0x28));

                Direct2BlockNo(out int X2, out int Y2, X, Y, NextRiverSectionDirection);
                int NextRiverAbsoluteAcre = D2toD1(X2, Y2);

                if (CheckCorrectBlockNo(X, Y, 1, 5, 1, 6) && AcreData[RiverAbsoluteStartAcre] != 0xE)
                {
                    if (CheckBlockGroup(AcreData[RiverAbsoluteStartAcre], 0))
                    {
                        byte RiverAlbum = RiverAlbuminCliff(AcreData[RiverAbsoluteStartAcre], 0x28);
                        if (RiverAlbum == 0xFF)
                        {
                            //Console.WriteLine("River Album in Cliff was invalid!");
                            return false;
                        }
                        else
                        {
                            int RiverAlbumAbsoluteAcre = D2toD1(X, Y + 1);
                            if (AcreData[RiverAlbumAbsoluteAcre] != 0xE)
                            {
                                RiverTraceState = 1;
                                AcreData[RiverAbsoluteStartAcre] = 0x28;
                                AcreX = X;
                                AcreY = Y;
                            }
                        }
                    }
                    else if (AcreData[NextRiverAbsoluteAcre] != 0xE)
                    {
                        AcreData[RiverAbsoluteStartAcre] = RiverStartType;
                        RiverTraceState = 1;
                        AcreX = X;
                        AcreY = Y;
                    }
                }

                if (RiverTraceState == 1)
                {
                    // Set River Border Acre & River Train Track Bridge Acre
                    int RiverBorderAcre = D2toD1(RiverXStartAcre, 0);
                    int RiverTrainTrackAcre = D2toD1(RiverXStartAcre, 1);
                    AcreData[RiverBorderAcre] = 1;
                    AcreData[RiverTrainTrackAcre] = 0xD;
                }
            }
            return true;
        }

        private bool TraceRiverPart2(ref byte[] AcreData, ref byte[] UnchangedAcreData, int AcreX, int AcreY, byte[] challenge_flag)
        {
            int RiverStartAcre = D2toD1(AcreX, AcreY);
            byte RiverStartAcreType = AcreData[RiverStartAcre];
            int RiverDirection = RiverIdx2NextDirect((byte)(RiverStartAcreType - 0x28));
            int X = 0, Y = 0;

            int RiverTraceState = 0;
            while (RiverTraceState == 0)
            {
                byte[] river_next_set = river_next_data[RiverStartAcreType - 0x28];
                byte NextRiverType = river_next_set[GetRandom(river_next_set.Length)];
                int NextRiverDirection = RiverIdx2NextDirect((byte)(NextRiverType - 0x28));

                Direct2BlockNo(out X, out Y, AcreX, AcreY, RiverDirection);
                if (Y == 6)
                {
                    NextRiverType = 0x28;
                    NextRiverDirection = RiverIdx2NextDirect(0);
                }

                Direct2BlockNo(out int X2, out int Y2, X, Y, NextRiverDirection);
                if (CheckCorrectBlockNo(X, Y, 1, 5, 1, 6))
                {
                    if (CheckCorrectBlockNo(X2, Y2, 1, 5, 1, 7))
                    {
                        int RiverPlacementAcre = D2toD1(X, Y);
                        int NextRiverPlacementAcre = D2toD1(X2, Y2);
                        if (UnchangedAcreData[NextRiverPlacementAcre] != 0xE)
                        {
                            bool PlacedBlock = false;
                            if (CheckBlockGroup(UnchangedAcreData[RiverPlacementAcre], 0))
                            {
                                byte RiverAlbum = RiverAlbuminCliff(UnchangedAcreData[RiverPlacementAcre], river_next_set[0]); // Check river_next_set
                                if (RiverAlbum != 0xFF)
                                {
                                    AcreData[RiverPlacementAcre] = river_next_set[0];
                                    PlacedBlock = true;
                                }
                                else
                                {
                                    //Console.WriteLine("River Album was invalid!");
                                    return false;
                                }
                            }
                            else
                            {
                                AcreData[RiverPlacementAcre] = NextRiverType;
                                PlacedBlock = true;
                            }

                            if (PlacedBlock)
                            {
                                if (Y2 == 7)
                                {
                                    RiverTraceState = 2;
                                }
                                else
                                {
                                    RiverTraceState = 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("River destination block was invalid!");
                    return false;
                }
            }

            if (RiverTraceState == 1)
            {
                return TraceRiverPart2(ref AcreData, ref UnchangedAcreData, X, Y, challenge_flag);
            }
            else if (RiverTraceState == 2)
            {

                if (GetCenterCrossZoneRiverCount(AcreData) != 0)
                {
                    /*
                    if (X == 1 || X == 5)
                    {
                        Console.WriteLine("River X value is invalid: " + X);
                    }
                    */
                    return X != 1 && X != 5;
                }
                else
                {
                    //Console.WriteLine("CrossZoneRiver Count was zero!");
                    return false;
                }
            }
            else
            {
                //Console.WriteLine("RiverTraceState wasn't valid!");
                return false;
            }
        }

        private bool LastCheckRiver(byte[] AcreData, int AcreX, int AcreY)
        {
            byte CurrentAcreType = AcreData[D2toD1(AcreX, AcreY)];
            while (CheckBlockGroup(CurrentAcreType, 1) == true)
            {
                int NextRiverDirection = RiverIdx2NextDirect(CurrentAcreType - 0x28);
                Direct2BlockNo(out int X, out int Y, AcreX, AcreY, NextRiverDirection);
                CurrentAcreType = AcreData[D2toD1(X, Y)];
                AcreX = X;
                AcreY = Y;
            }

            int Valid = AcreY ^ 6;
            Valid = (Valid >> 1) - (Valid & AcreY);
            return ((Valid >> 31) & 1) == 1; // This can be simplified as AcreY >= 7. I'm sticking to the code, though.
        }

        private bool DecideRiverAlbuminCliff(ref byte[] CliffData, ref byte[] RiverCliffData)
        {
            for (int Y = 0; Y < 8; Y++)
            {
                for (int X = 0; X < 7; X++)
                {
                    int AbsoluteAcre = D2toD1(X, Y);
                    byte RiverBlock = RiverCliffData[AbsoluteAcre];
                    byte SelectedAcreType = RiverAlbuminCliff(CliffData[AbsoluteAcre], RiverBlock);

                    if (SelectedAcreType == 0xFF)
                    {
                        // Check if the current block is a river block. If so, copy it to the "main" block data.
                        if (CheckBlockGroup(RiverBlock, 1) == true || RiverBlock == 1 || RiverBlock == 0xD)
                        {
                            CliffData[AbsoluteAcre] = RiverCliffData[AbsoluteAcre];
                        }
                    }
                    else
                    {
                        CliffData[AbsoluteAcre] = SelectedAcreType; // Set the block to a waterfall type.
                    }
                }
            }
            return true;
        }

        private bool DecideBaseRiver(ref byte[] AcreData, out byte[] RiverData)
        {
            byte[] UnchangedAcreData = new byte[AcreData.Length];
            Array.Copy(AcreData, UnchangedAcreData, AcreData.Length);
            RiverData = AcreData;
            AcreData = UnchangedAcreData;
            if (TraceRiverPart1(ref RiverData, out int AcreX, out int AcreY))
            {
                if (TraceRiverPart2(ref RiverData, ref AcreData, AcreX, AcreY, new byte[0x38]))
                {
                    return LastCheckRiver(RiverData, AcreX, AcreY);
                }
            }
            return false;
        }

        private bool SetRandomBlockData(ref byte[] AcreData, out byte[] RiverData) // Technically takes two copies of AcreData
        {
            RiverData = null;
            if (DecideBaseCliff(ref AcreData))
            {
                return DecideBaseRiver(ref AcreData, out RiverData);
            }
            return false;
        }

        private byte[] MakeBaseLandformStep2()
        {
            byte[] AcreData = new byte[70];
            byte[] RiverData;
            Array.Copy(DefaultTownStructure, AcreData, 70);
            while (SetRandomBlockData(ref AcreData, out RiverData) == false)
            {
                Array.Copy(DefaultTownStructure, AcreData, 70);
            }

            DecideRiverAlbuminCliff(ref AcreData, ref RiverData);
            return AcreData;
        }

        private byte[] MakeBaseLandformStep3()
        {
            byte[] data = new byte[step3_blockss[0].Length];
            Buffer.BlockCopy(step3_blockss[GetRandom(10)], 0, data, 0, data.Length);
            return data;
        }

        private byte[] MakeBaseLandform(int StepMode)
        {
            if (StepMode == 1)
            {
                return MakeBaseLandformStep3();
            }
            else
            {
                return MakeBaseLandformStep2();
            }
        }

        // Grass Blocks
        /// <summary>
        /// Creates a map of North & South acres for cliffs, & East & West acres for rivers.
        /// </summary>
        /// <param name="AcreData">Current accre data</param>
        /// <param name="river_left_right_info">River right & left acre map</param>
        /// <param name="cliff_up_down_info">Cliff up & down acre map</param>
        private void MakeFlatPlaceInformation(byte[] AcreData, out ushort[] river_left_right_info, out ushort[] cliff_up_down_info)
        {
            river_left_right_info = new ushort[70];
            cliff_up_down_info = new ushort[70];

            for (int i = 0; i < 70; i++)
            {
                river_left_right_info[i] = 2;
                cliff_up_down_info[i] = 2;
            }

            // Cliff Check
            for (int X = 1; X < 6; X++)
            {
                ushort StoreValue = 0;
                for (int Y = 1; Y < 9; Y++)
                {
                    if (StoreValue == 0 && CheckBlockGroup(AcreData[D2toD1(X, Y)], 8) == true)
                    {
                        StoreValue = 1;
                    }
                    cliff_up_down_info[D2toD1(X, Y)] = StoreValue;
                }
            }

            // River Check
            for (int Y = 1; Y < 9; Y++)
            {
                ushort StoreValue = 0;
                for (int X = 1; X < 6; X++)
                {
                    if (StoreValue == 0 && ((CheckBlockGroup(AcreData[D2toD1(X, Y)], 1) == true || CheckBlockGroup(AcreData[D2toD1(X, Y)], 4) == true)))
                    {
                        StoreValue = 1;
                    }
                    river_left_right_info[D2toD1(X, Y)] = StoreValue;
                }
            }
        }

        // Oceanfront Blocks
        private void SetMarinBlock(ref byte[] AcreData)
        {
            for (int X = 1; X < 6; X++)
            {
                int AcreIndex = D2toD1(X, 6);
                if (AcreData[AcreIndex] == 0x27)
                {
                    AcreData[AcreIndex] = 0x3F;
                }
                else if (AcreData[AcreIndex] == 0x28)
                {
                    AcreData[AcreIndex] = 0x40;
                }
            }

            // Set border blocks
            AcreData[D2toD1(0, 6)] = 0x50;
            AcreData[D2toD1(6, 6)] = 0x51;
        }

        // Bridges & Slopes
        /// <summary>
        /// Counts the number of times the river & cliffs cross paths.
        /// </summary>
        /// <param name="AcreX">The X-Acre of the first cliff-river crossing.</param>
        /// <param name="AcreY">The Y-Acre of the first cliff-river crossing.</param>
        /// <param name="AcreData">The current acre data.</param>
        /// <returns>The amount of times the river & cliffs cross.</returns>
        public int GetRiverCrossCliffInfo(out int AcreX, out int AcreY, byte[] AcreData)
        {
            int AcreIdx = 0;
            int Count = 0;

            AcreX = AcreY = 0;

            for (int i = 0; i < 0x38; i++)
            {
                int Y = i / 7;
                int X = i % 7;

                for (int x = 0; x < 7; x++)
                {
                    if (AcreData[AcreIdx] == cross_data[x])
                    {
                        if (Count == 0)
                        {
                            AcreX = X;
                            AcreY = Y;
                        }
                        Count++;
                    }
                }

                AcreIdx++;
            }

            return Count;
        }

        private int SetBridgeBlock(ref byte[] AcreData, bool ThreeLayeredTown)
        {
            bool PlaceUpperBridge = (GetRandom(10) & 1) == 1;
            GetRiverCrossCliffInfo(out int AcreX, out int AcreY, AcreData);
            int AcreIdx = 0;
            int ValidBridgePlaceUpper = 0;
            int ValidBridgePlaceLower = 0;
            int SetBridgeBlockBit = 0;

            for (int Y = 0; Y < 8; Y++)
            {
                for (int X = 0; X < 7; X++)
                {
                    if (CheckBlockGroup(AcreData[AcreIdx], 1) == true)
                    {
                        if (Y >= AcreY)
                        {
                            ValidBridgePlaceUpper++;
                        }
                        else
                        {
                            ValidBridgePlaceLower++;
                        }
                    }
                    AcreIdx++;
                }
            }

            // Lower area first
            if (ValidBridgePlaceLower != 0)
            {
                int BridgeLowerLocation = GetRandom(ValidBridgePlaceLower);
                AcreIdx = 0;
                int Count = 0;

                for (int Y = 0; Y < 8; Y++)
                {
                    for (int X = 0; X < 7; X++)
                    {
                        if (CheckBlockGroup(AcreData[AcreIdx], 1) == true)
                        {
                            if (Y < AcreY)
                            {
                                if (Count == BridgeLowerLocation)
                                {
                                    SetBridgeBlockBit |= (1 << 2); // 4
                                    AcreData[AcreIdx] = (byte)(AcreData[AcreIdx] + 7);
                                }
                                Count++;
                            }
                        }
                        AcreIdx++;
                    }
                }
            }

            // Upper area next
            if (ValidBridgePlaceUpper != 0 && ThreeLayeredTown == false && PlaceUpperBridge == true)
            {
                int BridgeUpperLocation = GetRandom(ValidBridgePlaceUpper);
                AcreIdx = 0;
                int Count = 0;

                for (int Y = 0; Y < 8; Y++)
                {
                    for (int X = 0; X < 7; X++)
                    {
                        if (CheckBlockGroup(AcreData[AcreIdx], 1) == true)
                        {
                            if (Y > AcreY)
                            {
                                if (Count == BridgeUpperLocation)
                                {
                                    SetBridgeBlockBit |= (1 << 3); // 8
                                    AcreData[AcreIdx] = (byte)(AcreData[AcreIdx] + 7);
                                }
                                Count++;
                            }
                        }
                        AcreIdx++;
                    }
                }
            }

            return SetBridgeBlockBit;
        }

        // Slope Code
        private int BlockType2CliffIndex(int CliffIdx)
        {
            int CliffBlockInfo = GetSystemBlockInfo(CliffIdx);
            for (int i = 0; i < 7; i++)
            {
                if ((CliffBlockInfo & cliff_info[i]) == cliff_info[i])
                {
                    return i;
                }
            }
            return -1;
        }

        private int CountDirectedInfoCliff(byte[] AcreData, int AcreX, int AcreY, int DesiredRiverSide)
        {
            AcreX += 1;
            byte CurrentBlockType = AcreData[D2toD1(AcreX, AcreY)];
            int RiverSide = 0;
            int ValidCliffBlocks = 0;

            while (CurrentBlockType != 0x3E)
            {
                int BlockCliffIndex = BlockType2CliffIndex(CurrentBlockType);
                if (CheckBlockGroup(CurrentBlockType, 4) == true) // Check if the current block is a waterfall acre
                {
                    RiverSide = 1;
                }
                else
                {
                    if (DesiredRiverSide == RiverSide)
                    {
                        ValidCliffBlocks++;
                    }
                }

                byte CliffDirection = cliff_next_direct[BlockCliffIndex];
                Direct2BlockNo(out int X, out int Y, AcreX, AcreY, CliffDirection);
                CurrentBlockType = AcreData[D2toD1(X, Y)];
                AcreX = X;
                AcreY = Y;
            }

            return ValidCliffBlocks;
        }

        private bool SetSlopeDirectedInfoCliff(ref byte[] AcreData, int AcreX, int AcreY, int ValidBlockType, int WriteIndex)
        {
            AcreX += 1;
            byte CurrentBlockType = AcreData[D2toD1(AcreX, AcreY)];
            int Unknown = 0;
            int Count = 0;

            while (CurrentBlockType != 0x3E)
            {
                int BlockCliffIndex = BlockType2CliffIndex(CurrentBlockType);
                if (CheckBlockGroup(CurrentBlockType, 4) == true)
                {
                    Unknown = 1;
                }
                else
                {
                    if (ValidBlockType == Unknown)
                    {
                        if (Count == WriteIndex)
                        {
                            AcreData[D2toD1(AcreX, AcreY)] = (byte)(BlockCliffIndex + 0x36);
                            return true;
                        }
                        else
                        {
                            Count++;
                        }
                    }
                }

                byte CliffDirection = cliff_next_direct[BlockCliffIndex];
                Direct2BlockNo(out int X, out int Y, AcreX, AcreY, CliffDirection);
                CurrentBlockType = AcreData[D2toD1(X, Y)];
                AcreX = X;
                AcreY = Y;
            }

            return false;
        }

        private int SetSlopeBlock(ref byte[] AcreData)
        {
            int SlopeBit = 0;
            for (int Y = 0; Y < 8; Y++)
            {
                byte BlockType = AcreData[D2toD1(0, Y)];
                if (BlockType == 0x3D)
                {
                    int Count = CountDirectedInfoCliff(AcreData, 0, Y, 0);
                    if (Count > 0)
                    {
                        int SlopeIndex = GetRandom(Count);
                        if (SetSlopeDirectedInfoCliff(ref AcreData, 0, Y, 0, SlopeIndex))
                        {
                            SlopeBit |= (1 << 0); // 1
                        }
                    }

                    Count = CountDirectedInfoCliff(AcreData, 0, Y, 1);
                    if (Count > 0)
                    {
                        int SlopeIndex = GetRandom(Count);
                        if (SetSlopeDirectedInfoCliff(ref AcreData, 0, Y, 1, SlopeIndex))
                        {
                            SlopeBit |= (1 << 1); // 2
                        }
                    }
                }
            }

            return SlopeBit;
        }

        private int SetBridgeAndSlopeBlock(ref byte[] AcreData, bool IsThreeLayeredTown)
        {
            return SetBridgeBlock(ref AcreData, IsThreeLayeredTown) | SetSlopeBlock(ref AcreData);
        }

        private int SetNeedleworkAndWharfBlock(ref byte[] AcreData)
        {
            int WorkBit = 0;
            int CurrentNeedleworkCheckIdx = 0;
            int NeedleworkXAcre = GetRandom(3);
            int WharfBlockIdx = D2toD1(5, 6);
            if (AcreData[WharfBlockIdx] == 0x3F)
            {
                AcreData[WharfBlockIdx] = 0x64;
                for (int X = 1; X < 6; X++)
                {
                    if (AcreData[D2toD1(X, 6)] == 0x3F)
                    {
                        if (NeedleworkXAcre == CurrentNeedleworkCheckIdx)
                        {
                            AcreData[D2toD1(X, 6)] = 0x55;
                            WorkBit |= (1 << 8); // 0x100
                        }
                        CurrentNeedleworkCheckIdx++;
                    }
                }
            }

            return WorkBit;
        }

        // Museum, Wishing Well, & Police Station Code

        // Man the devs really really made this terrible
        private bool JudgeFlatBlock(byte[] AcreData, int Index, RiverSide RiverDirection, CliffSide CliffDirection, ushort[] cliff_up_down_info, ushort[] river_left_right_info)
        {
            if ((int)RiverDirection > -1 && (int)RiverDirection < 3 && (int)CliffDirection > -1 && (int)CliffDirection < 3)
            {
                if (Index > 0 && Index < 0x38)
                {
                    if (AcreData[Index] == 0x27)
                    {
                        if (CliffDirection == CliffSide.Any)
                        {
                            if (RiverDirection == RiverSide.Any)
                            {
                                if ((CliffSide)cliff_up_down_info[Index] == CliffDirection)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if ((RiverSide)river_left_right_info[Index] == RiverDirection)
                                {
                                    if ((CliffSide)cliff_up_down_info[Index] == CliffDirection)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (RiverDirection == RiverSide.Any)
                            {
                                if ((CliffSide)cliff_up_down_info[Index] == CliffDirection)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if ((RiverSide)river_left_right_info[Index] == RiverDirection)
                                {
                                    if ((CliffSide)cliff_up_down_info[Index] == CliffDirection)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private int CountFlatBlock(byte[] AcreData, RiverSide RiverDirection, CliffSide CliffDirection, ushort[] cliff_up_down_info, ushort[] river_left_right_info)
        {
            int FlatBlocks = 0;
            for (int i = 0; i < 0x38; i++)
            {
                if (JudgeFlatBlock(AcreData, i, RiverDirection, CliffDirection, cliff_up_down_info, river_left_right_info) == true)
                {
                    FlatBlocks++;
                }
            }
            return FlatBlocks;
        }

        private int RewriteFlatType(ref byte[] AcreData, int FlatBlockIndex, byte NewFlatBlockType, RiverSide RiverDirection, CliffSide CliffDirection,
            ushort[] cliff_up_down_info, ushort[] river_left_right_info)
        {
            int FlatBlock = 0;
            for (int i = 0; i < 0x38; i++)
            {
                if (JudgeFlatBlock(AcreData, i, RiverDirection, CliffDirection, cliff_up_down_info, river_left_right_info) == true)
                {
                    if (FlatBlock == FlatBlockIndex)
                    {
                        AcreData[i] = NewFlatBlockType;
                        return i;
                    }
                    FlatBlock++;
                }
            }

            return -1;
        }

        private bool FlatBlock2Unique(ref byte[] AcreData, byte NewFlatBlockType, RiverSide RiverDirection, CliffSide CliffDirection,
            ushort[] cliff_up_down_info, ushort[] river_left_right_info)
        {
            int FlatBlocks = CountFlatBlock(AcreData, RiverDirection, CliffDirection, cliff_up_down_info, river_left_right_info);
            if (FlatBlocks > 0)
            {
                if (RewriteFlatType(ref AcreData, GetRandom(FlatBlocks), NewFlatBlockType, RiverDirection, CliffDirection, cliff_up_down_info, river_left_right_info) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        private int SetUniqueFlatBlock(ref byte[] AcreData, ushort[] cliff_up_down_info, ushort[] river_left_right_info)
        {
            int FlatBit = 0;

            var RiverSide = (RiverSide)(GetRandom(100) & 1);
            var OppositeRiverSide = (RiverSide)(((int)RiverSide ^ 1) & 1);
            if (FlatBlock2Unique(ref AcreData, 0x42, RiverSide, CliffSide.Down, cliff_up_down_info, river_left_right_info) == true)
            {
                FlatBit |= 0x10;
            }
            else if (FlatBlock2Unique(ref AcreData, 0x42, OppositeRiverSide, CliffSide.Down, cliff_up_down_info, river_left_right_info) == true)
            {
                FlatBit |= 0x10;
            }

            if (FlatBlock2Unique(ref AcreData, 0x44, OppositeRiverSide, CliffSide.Down, cliff_up_down_info, river_left_right_info) == true)
            {
                FlatBit |= 0x20;
            }
            else if (FlatBlock2Unique(ref AcreData, 0x44, RiverSide, CliffSide.Down, cliff_up_down_info, river_left_right_info) == true)
            {
                FlatBit |= 0x20;
            }

            if (FlatBlock2Unique(ref AcreData, 0x54, RiverSide.Any, CliffSide.Down, cliff_up_down_info, river_left_right_info) == true)
            {
                FlatBit |= 0x40;
            }

            return FlatBit;
        }

        // Lake Code
        private int CountPureRiver(byte[] AcreData)
        {
            int RiverAcres = 0;
            for (int i = 0; i < 56; i++)
            {
                if (AcreData[i] == 0x28 || (byte)(AcreData[i] - 0x29) <= 4 || AcreData[i] == 0x2E)
                {
                    RiverAcres++;
                }
            }

            return RiverAcres;
        }

        private bool SetPoolDirectedRiverBlock(ref byte[] AcreData, int LakeRiverIndex)
        {
            int RiverAcre = 0;
            for (int i = 0; i < 0x38; i++)
            {
                // 28
                // 29
                // 2A
                // 2B
                // 2C
                // 2D
                if (AcreData[i] == 0x28 || (byte)(AcreData[i] - 0x29) <= 4 || AcreData[i] == 0x2E)
                {
                    if (RiverAcre == LakeRiverIndex)
                    {
                        AcreData[i] += 0x1D;
                        return true;
                    }
                    else
                    {
                        RiverAcre++;
                    }
                }
            }
            return false;
        }

        private int SetPoolBlock(ref byte[] AcreData)
        {
            int RiverAcres = CountPureRiver(AcreData);
            if (RiverAcres > 0)
            {
                int LakeAcre = GetRandom(RiverAcres);
                if (SetPoolDirectedRiverBlock(ref AcreData, LakeAcre) == true)
                {
                    return 0x80;
                }
            }
            return 0;
        }

        // Oceanfront Bridge
        private int SetSeaBlockWithBridgeRiver(ref byte[] AcreData, int CurrentGenerationBit)
        {
            if ((CurrentGenerationBit & 8) == 0) // Make sure the lower bridge wasn't placed already
            {
                for (int i = 0; i < 0x38; i++)
                {
                    if (AcreData[i] == 0x40)
                    {
                        AcreData[i] = 0x52;
                        return 8;
                    }
                }
            }
            return 0;
        }

        private ushort GetExceptionalSeaBgDownBgName(ushort BgType)
        {
            ushort CurrentValue = 0;
            ushort CurrentIdx = 0;
            do
            {
                CurrentValue = exceptional_table[CurrentIdx];
                if (CurrentValue != 0x125)
                {
                    if (CurrentValue == BgType)
                    {
                        return exceptional_table[CurrentIdx + 1];
                    }
                }
                CurrentIdx += 2;
            } while (CurrentValue != 0x125);
            return BgType;
        }

        private ushort BgName2RandomConbiNo(ushort ExceptionalValue)
        {
            int Matches = 0;
            for (int i = 0; i < data_combi_table_number; i++)
            {
                if (ExceptionalValue == data_combi_table[i].bg_name && data_combi_table[i].type != 0xFF)
                {
                    Matches++;
                }
            }

            if (Matches > 0)
            {
                int CurrentMatch = 0;
                int RandomlySelectedMatch = GetRandom(0); // Silly Animal Crossing Devs. This isn't random.
                for (int i = 0; i < data_combi_table_number; i++)
                {
                    if (data_combi_table[i].bg_name == ExceptionalValue && data_combi_table[i].type != 0xFF)
                    {
                        if (CurrentMatch == RandomlySelectedMatch)
                        {
                            return (ushort)i;
                        }
                        else
                        {
                            CurrentMatch++;
                        }
                    }
                }
            }

            return data_combi_table_number;
        }

        private bool SearchAlreadyUse(ushort acre, in ushort[] acres)
        {
            return Array.IndexOf(acres, acre) != -1;
        }

        private int TypeCombCount(int cliff_b, bool not_unique, in ushort[] acres)
        {
            int count = 0;
            for (ushort i = 0; i < data_combi_table.Length; i++)
            {
                data_combi t = data_combi_table[i];
                if (t.type == cliff_b)
                {
                    if (not_unique || !SearchAlreadyUse(i, acres))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void SetUniqueRailBlock(ref byte[] Data)
        {
            byte A = 0, B = 0;
            bool ASet = false, BSet = false;

            if ((GetRandom(1000) & 1) == 0)
            {
                A = 0x43;
                B = 0x41;
            }
            else
            {
                A = 0x41;
                B = 0x43;
            }

            // Set A
            while (!ASet)
            {
                int ALocation = GetRandom(2);
                if (Data[8 + ALocation] == 0x0C)
                {
                    Data[8 + ALocation] = A;
                    ASet = true;
                }
            }

            // Set B
            while (!BSet)
            {
                int BLocation = GetRandom(2);
                if (Data[11 + BLocation] == 0x0C)
                {
                    Data[11 + BLocation] = B;
                    BSet = true;
                }
            }
        }

        private void ReportRandomFieldBitResult(int RandomFieldBit, int PerfectBit)
        {
            Console.WriteLine(string.Format("RandomField Bit: {0} | Perfect Bit: {1}", RandomFieldBit.ToString("X2"), PerfectBit.ToString("X2")));
        }

        // Acre Height Code
        private byte[] InitBlockBase()
            => new byte[70];

        private void GetBlockBase(ref byte[] HeightTable, byte[] AcreData)
        {
            for (int X = 0; X < 7; X++)
            {
                byte CurrentHeight = 0;
                for (int Y = 9; Y > -1; Y--)
                {
                    byte CurrentBlock = AcreData[D2toD1(X, Y)];
                    HeightTable[D2toD1(X, Y)] = CurrentHeight;
                    if ((GetSystemBlockInfo(CurrentBlock) & 1) == 1 || (GetSystemBlockInfo(CurrentBlock) & 8) == 8 || (GetSystemBlockInfo(CurrentBlock) & 0x10) == 0x10
                        || CurrentBlock == 0x3D || CurrentBlock == 0x3E)
                    {
                        CurrentHeight++;
                    }
                }
            }
        }

        private byte[] MakeBaseHeightTable(byte[] AcreData)
        {
            byte[] HeightTable = new byte[70]; //InitBlockBase();
            GetBlockBase(ref HeightTable, AcreData);
            return HeightTable;
        }

        public int min_gen_fqrand_calls = int.MaxValue;
        public int max_gen_fqrand_calls = 0;
        public long total_fqrand_calls = 0;
        public Dictionary<int, int> calls_dict = new();

        private bool MakeRandomField_ovl_OPTIMAL(int well, int lake, int police, int museum)
        {
#if DEBUG
            fqrand_calls = 0;
#endif
            // TODO: this should return a bool and only return true if Nook's & Post Office are in A-2/A-4
            // We're going to toss out 3-tiered towns for optimization.
            int StepMode = GetRandomStepMode();

            if (StepMode == 0)
            {
                int Bit = 0;
                byte[] AcreData = null;
                byte[] HeightTable;

                while ((PERFECT_BIT_CONSTEXPR & Bit) != PERFECT_BIT_CONSTEXPR)
                {
                    AcreData = MakeBaseLandformStep2();
                    MakeFlatPlaceInformation(AcreData, out ushort[] river_left_right_info, out ushort[] cliff_up_down_info);
                    SetMarinBlock(ref AcreData);
                    Bit = SetBridgeAndSlopeBlock(ref AcreData, StepMode == 1);
                    Bit |= SetNeedleworkAndWharfBlock(ref AcreData);
                    Bit |= SetUniqueFlatBlock(ref AcreData, cliff_up_down_info, river_left_right_info);
                    SetUniqueRailBlock(ref AcreData);
                    Bit |= SetPoolBlock(ref AcreData);
                    if (Bit == 0x1F7)
                    {
                        Bit |= SetSeaBlockWithBridgeRiver(ref AcreData, Bit);
                        HeightTable = MakeBaseHeightTable(AcreData);
                    }
                }

#if DEBUG
                total_fqrand_calls += fqrand_calls;
                if (fqrand_calls < min_gen_fqrand_calls)
                {
                    min_gen_fqrand_calls = fqrand_calls;
                }
                if (fqrand_calls > max_gen_fqrand_calls)
                {
                    max_gen_fqrand_calls = fqrand_calls;
                }
                if (!calls_dict.ContainsKey(fqrand_calls))
                {
                    calls_dict.Add(fqrand_calls, 1);
                }
                else
                {
                    calls_dict[fqrand_calls]++;
                }
#endif
                return
                    AcreData[lake] >= 0x45 && AcreData[lake] <= 0x4B &&
                    AcreData[well] == 0x42 &&
                    AcreData[police] == 0x44 &&
                    AcreData[museum] == 0x54;
            }

            return false;
        }

        private Tuple<byte[], byte[]> MakeRandomField_ovl()
        {
            int StepMode = GetRandomStepMode();

            
#if DEBUG
            //int PerfectBit = MakePerfectBit();
            const int PerfectBit = PERFECT_BIT_CONSTEXPR;
#else
            const int PerfectBit = PERFECT_BIT_CONSTEXPR;
#endif
            int Bit = 0;
            byte[] AcreData = null;
            byte[] HeightTable = null;
            //Console.WriteLine("StepMode: " + StepMode);

            while ((PerfectBit & Bit) != PerfectBit)
            {
                AcreData = HeightTable = null;
                AcreData = MakeBaseLandform(StepMode);
                MakeFlatPlaceInformation(AcreData, out ushort[] river_left_right_info, out ushort[] cliff_up_down_info);
                SetMarinBlock(ref AcreData);
                Bit = SetBridgeAndSlopeBlock(ref AcreData, StepMode == 1);
                Bit |= SetNeedleworkAndWharfBlock(ref AcreData);
                Bit |= SetUniqueFlatBlock(ref AcreData, cliff_up_down_info, river_left_right_info);
                SetUniqueRailBlock(ref AcreData);
                Bit |= SetPoolBlock(ref AcreData);
                Bit |= SetSeaBlockWithBridgeRiver(ref AcreData, Bit);
                HeightTable = MakeBaseHeightTable(AcreData);

#if DEBUG
            //ReportRandomFieldBitResult(Bit, PerfectBit);
#endif
            }
            return new Tuple<byte[], byte[]>(AcreData, HeightTable);
        }

        private ushort IndexInType2BlockNo(ushort type, int idx, bool not_unique, in ushort[] acres)
        {
            int count = 0;

            for (ushort i = 0; i < data_combi_table.Length; i++)
            {
                data_combi combi = data_combi_table[i];
                if (combi.type == type)
                {
                    if (not_unique || !SearchAlreadyUse(i, acres))
                    {
                        if (count == idx)
                        {
                            return i;
                        }
                        count++;
                    }
                }
            }

            return 0xFFFF;
        }

        public bool GenerateBGOptimal(int well, int lake, int police, int museum)
        {
            return MakeRandomField_ovl_OPTIMAL(well, lake, police, museum);
        }

        public ushort[] GenerateBG()
        {
            var RandomFieldData = MakeRandomField_ovl();
            byte[] Data = RandomFieldData.Item1;
            byte[] HeightData = RandomFieldData.Item2;

            ushort[] AcreData = new ushort[70];
            ushort[] use_data = new ushort[70];
            for (int i = 0; i < 70; i++)
            {
                use_data[i] = 0xFFFF;
            }

            for (int i = 0; i < 70; i++)
            {
                ushort BlockId;
                if (Data[i] == 0x65)
                {
                    ushort AboveAcreId = AcreData[D2toD1(i % 7, (i / 7) - 1)];
                    int AcreBlockId = AboveAcreId >> 2;
                    ushort OceanId = data_combi_table[AcreBlockId].bg_name;
                    ushort ExceptionalValue = GetExceptionalSeaBgDownBgName(OceanId);
                    ushort CorrectBgValue = BgName2RandomConbiNo(ExceptionalValue);
                    BlockId = (ushort)(CorrectBgValue << 2);
                }
                else
                {
                    int type_count = TypeCombCount(Data[i], false, use_data);
                    if (type_count == 0)
                    {
                        ushort block_no = IndexInType2BlockNo(Data[i], GetRandom(TypeCombCount(Data[i], true, use_data)), true, use_data);
                        if (block_no == 0xFFFF)
                        {
                            BlockId = 0x284;
                        }
                        else
                        {
                            BlockId = (ushort)(block_no << 2);
                            use_data[i] = block_no;
                        }
                    }
                    else
                    {
                        ushort block_no = IndexInType2BlockNo(Data[i], GetRandom(type_count), false, use_data);
                        if (block_no == 0xFFFF)
                        {
                            block_no = IndexInType2BlockNo(Data[i], GetRandom(TypeCombCount(Data[i], true, use_data)), true, use_data);
                            if (block_no == 0xFFFF)
                            {
                                BlockId = 0x284;
                            }
                            else
                            {
                                BlockId = (ushort)(block_no << 2);
                                use_data[i] = block_no;
                            }
                        }
                        else
                        {
                            BlockId = (ushort)(block_no << 2);
                            use_data[i] = block_no;
                        }
                    }
                }
                AcreData[i] = (ushort)(BlockId | HeightData[i]);
            }

#if DEBUG
            //Console.WriteLine($"fqrand calls: {fqrand_calls}");
#endif
            return AcreData;
        }

        private static ushort Swap16(ushort i)
        {
            return (ushort)(((i & 0xFF) << 8) | ((i >> 8) & 0xFF));
        }

        private static uint Swap32(uint i)
        {
            return (uint)(((i & 0xFF) << 24) | ((i & 0xFF00) << 8) | ((i >> 8) & 0xFF00) | ((i >> 24) & 0xFF));
        }

        private static void PullTreeUT(in ushort[] block, int ut_pos)
        {
            ushort name = block[ut_pos];
            if ((name < 0x800 || name > 0x83B) && (name < 0x84F || name > 0x85B) && (name < 0x85D || name > 0x861)) {
                if (name < 0x863 || name > 0x868)
                {
                    return;
                }
            }

            //Console.WriteLine("Pulled tree");
            block[ut_pos] = 0;
        }

        private static void PullTreeBlock(in ushort[] block, int ut)
        {
            for (; ut < 16 * 16; ut += 16)
            {
                PullTreeUT(block, ut);
            }
        }

        private static void PullTree(in ushort[][] fg)
        {
            for (int row = 0; row < 6; row++)
            {
                PullTreeBlock(fg[row * 5 + 0], 0);
                PullTreeBlock(fg[row * 5 + 4], 15);
            }
        }

        private static void PullTreeUnderPlayerBlock(in ushort[][] fg)
        {
            PullTreeUT(fg[2 * 5 + 2], 0 * 16 + 7);
            PullTreeUT(fg[2 * 5 + 2], 0 * 16 + 8);
            PullTreeUT(fg[2 * 5 + 2], 1 * 16 + 7);
            PullTreeUT(fg[2 * 5 + 2], 1 * 16 + 8);
        }

        private void ChangeItemBlock(in ushort[] items, ushort new_item, ushort item, int total)
        {
            int target = (int)(fqrand() * total);
            for (int i = 0; i < 16 * 16; i++)
            {
                if (items[i] == item)
                {
                    if (target == 0)
                    {
                        items[i] = new_item;
                        return;
                    }
                    target--;
                }
            }
        }

        private static int GetItemNumInBlock(ushort item, in ushort[] items)
        {
            int count = 0;
            for (int i = 0; i < 16 * 16; i++)
            {
                if (items[i] == item)
                {
                    count++;
                }
            }

            return count;
        }

        private void ChangeTree2OtherBlock(in ushort[] items, ushort new_item, ushort item, int num)
        {
            int count = GetItemNumInBlock(item, items);
            while (num > 0 && count != 0)
            {
                ChangeItemBlock(items, new_item, item, count);
                count--;
                num--;
            }
        }

        private void ChangeTree2FruitTreeBlock(in ushort[] items, ushort tree)
        {
            ChangeTree2OtherBlock(items, tree, 0x0804, 1);
        }

        private void ChangeTree2FruitTreeLine(int row, in ushort[][] acres, ushort tree)
        {
            int block_0 = (int)(fqrand() * 5.0f);
            int block_1 = (int)(fqrand() * 4.0f);

            if (block_1 == block_0)
            {
                block_1++;
            }

            ChangeTree2FruitTreeBlock(acres[row * 5 + block_0], tree);
            ChangeTree2FruitTreeBlock(acres[row * 5 + block_1], tree);
        }

        private static readonly ushort[] l_tree_max_table = { 0x080C, 0x082C, 0x0824, 0x081C, 0x0814 };

        private void ChangeTree2FruitTree(int fruit_idx, in ushort[][] acres)
        {
            for (int i = 0; i < 6; i++)
            {
                ChangeTree2FruitTreeLine(i, acres, l_tree_max_table[fruit_idx]);
            }
        }

        private static readonly int[] cedar_max_table = { 6, 4, 2, 0 };

        private void ChangeTree2Cedar(in ushort[][] fg)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    ChangeTree2OtherBlock(fg[row * 5 + column], 0x0861, 0x0804, cedar_max_table[row]);
                }
            }
        }

        /// <summary>
        /// Generates the foreground (fg) (items) tiles based on supplied background (bg) (acre) tiles.
        /// </summary>
        /// <param name="bg">The background acre tiles</param>
        /// <param name="fruit_idx">The town fruit calculated earlier</param>
        /// <returns>ushort[] fg_data</returns>
        public ushort[][] GenerateFG(in ushort[] bg, int fruit_idx)
        {
            ushort[][] fg = new ushort[5 * 6][];

            /* Select actual items for our bg combination */
            int fg_idx = 0;
            for (int i = 0; i < bg.Length; i++)
            {
                int x = i % 7;
                int y = i / 7;

                if (x > 0 && x < 6 && y > 0 && y < 7)
                {
                    fg[fg_idx++] = fg_data[data_combi_table[bg[i] >> 2].fg_name];
                }
            }

            /* Remove trees */
            PullTree(fg);
            PullTreeUnderPlayerBlock(fg);

            /* Change trees to fruit trees then cedar trees */
            ChangeTree2FruitTree(fruit_idx, fg);
            ChangeTree2Cedar(fg);

            return fg;
        }

        private sealed class bgdata_item
        {
            public ushort bg_name;

            /* mFM_bgdd_c dispinfo - 24 bytes*/

            public uint[,] bg_utinfo = new uint[16, 16]; /* 16x16, 1024 bytes total */

            /* mFM_bgssd_c sound_source[6] - 24 bytes */

            public bgdata_item(BinaryReader reader)
            {
                bg_name = Swap16(reader.ReadUInt16());
                reader.BaseStream.Seek(sizeof(ushort) + 24, SeekOrigin.Current); /* skip to bg_utinfo */
                for (int ut_z = 0; ut_z < 16; ut_z++)
                {
                    for (int ut_x = 0; ut_x < 16; ut_x++)
                    {
                        bg_utinfo[ut_z, ut_x] = Swap32(reader.ReadUInt32());
                    }
                }
                reader.BaseStream.Seek(24, SeekOrigin.Current); /* skip sound source data */
            }
        }

        private static bgdata_item[] LoadBGData()
        {

            using BinaryReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("AC_fqrand_solver.Resources.bgdata.bin"));
            int count = (int)reader.BaseStream.Length / 0x434;
            bgdata_item[] sorted_bgdata = new bgdata_item[0x125];

            for (int i = 0; i < count; i++)
            {
                bgdata_item bg_data = new(reader);
                if (bg_data.bg_name < 0x125)
                {
                    // NOTE: game has a bug here: buffer can hold 0x125 entries but they check bg_name < 0x126 which would be 0x126 entries
                    sorted_bgdata[bg_data.bg_name] = bg_data;
                }
            }

            return sorted_bgdata;
        }

        private static bool CheckBlockKind_OR(int combi, int block_kind)
        {
            return (mRF_block_info[data_combi_table[combi].type] & block_kind) != 0;
        }

        private enum bg_unit_attribute
        {
            mAtr_GRASS,
            mAtr_GRASS2,
            mAtr_GRASS3,
            mAtr_GRASS4,
            mAtr_SOIL,
            mAtr_SOIL2,
            mAtr_SOIL3,
            mAtr_STONE,
            mAtr_HOME,
            mAtr_YABU,
            mAtr_SANDHOLE,
            mAtr_WAVE,
            mAtr_WATER,
            mAtr_FALL,
            mAtr_RIVER_N,
            mAtr_RIVER_NW,
            mAtr_RIVER_W,
            mAtr_RIVER_SW,
            mAtr_RIVER_S,
            mAtr_RIVER_SE,
            mAtr_RIVER_E,
            mAtr_RIVER_NE,
            mAtr_SAND,
            mAtr_WOOD,
            mAtr_SEA,
            mAtr_WAVE_SE2,
            mAtr_WAVE_SW2,
            mAtr_WOODB_NW,
            mAtr_WOODB_SW,
            mAtr_WOODB_SE,
            mAtr_WOODB_NE,
            mAtr_WOODB_UNDER,
            mAtr_STNB_N,
            mAtr_STNB_E,
            mAtr_STNB_W,
            mAtr_STNB_S,
            mAtr_WAVE_S,
            mAtr_WAVE_SE,
            mAtr_WAVE_SW,
            mAtr_G3_W_NW,
            mAtr_G3_W_SW,
            mAtr_G3_W_SE,
            mAtr_G3_W_NE,
            mAtr_G3_N,
            mAtr_G3_E,
            mAtr_G3_W,
            mAtr_G3_S,
            mAtr_G3_N_ND,
            mAtr_G3_E_ND,
            mAtr_G3_W_ND,
            mAtr_G3_S_ND,
            mAtr_G3_NaW_ND,
            mAtr_G3_SaW_ND,
            mAtr_G3_SaE_ND,
            mAtr_G3_NaE_ND,
            mAtr_G3_NW,
            mAtr_G3_SW,
            mAtr_G3_SE,
            mAtr_G3_NE,
            mAtr_G3_NaW,
            mAtr_G3_SaW,
            mAtr_G3_SaE,
            mAtr_G3_NaE,
            mAtr_SLATE,
            mAtr_ATR_COUNT
        }

        private static bool mCoBG_CheckHole_OrgAttr(int ut_attribute)
        {
            if (ut_attribute < 0) return false;
            return (bg_unit_attribute)ut_attribute switch
            {
                bg_unit_attribute.mAtr_GRASS => true,
                bg_unit_attribute.mAtr_GRASS2 => true,
                bg_unit_attribute.mAtr_GRASS3 => true,
                bg_unit_attribute.mAtr_GRASS4 => false,
                bg_unit_attribute.mAtr_SOIL => true,
                bg_unit_attribute.mAtr_SOIL2 => true,
                bg_unit_attribute.mAtr_SOIL3 => true,
                bg_unit_attribute.mAtr_STONE => false,
                bg_unit_attribute.mAtr_HOME => false,
                bg_unit_attribute.mAtr_YABU => false,
                bg_unit_attribute.mAtr_SANDHOLE => true,
                bg_unit_attribute.mAtr_WAVE => false,
                bg_unit_attribute.mAtr_WATER => false,
                bg_unit_attribute.mAtr_FALL => false,
                bg_unit_attribute.mAtr_RIVER_N => false,
                bg_unit_attribute.mAtr_RIVER_NW => false,
                bg_unit_attribute.mAtr_RIVER_W => false,
                bg_unit_attribute.mAtr_RIVER_SW => false,
                bg_unit_attribute.mAtr_RIVER_S => false,
                bg_unit_attribute.mAtr_RIVER_SE => false,
                bg_unit_attribute.mAtr_RIVER_E => false,
                bg_unit_attribute.mAtr_RIVER_NE => false,
                bg_unit_attribute.mAtr_SAND => true,
                bg_unit_attribute.mAtr_WOOD => false,
                bg_unit_attribute.mAtr_SEA => false,
                bg_unit_attribute.mAtr_WAVE_SE2 => true,
                bg_unit_attribute.mAtr_WAVE_SW2 => true,
                bg_unit_attribute.mAtr_WOODB_NW => false,
                bg_unit_attribute.mAtr_WOODB_SW => false,
                bg_unit_attribute.mAtr_WOODB_SE => false,
                bg_unit_attribute.mAtr_WOODB_NE => false,
                bg_unit_attribute.mAtr_WOODB_UNDER => false,
                bg_unit_attribute.mAtr_STNB_N => false,
                bg_unit_attribute.mAtr_STNB_E => false,
                bg_unit_attribute.mAtr_STNB_W => false,
                bg_unit_attribute.mAtr_STNB_S => false,
                bg_unit_attribute.mAtr_WAVE_S => true,
                bg_unit_attribute.mAtr_WAVE_SE => false,
                bg_unit_attribute.mAtr_WAVE_SW => false,
                bg_unit_attribute.mAtr_G3_W_NW => false,
                bg_unit_attribute.mAtr_G3_W_SW => false,
                bg_unit_attribute.mAtr_G3_W_SE => false,
                bg_unit_attribute.mAtr_G3_W_NE => false,
                bg_unit_attribute.mAtr_G3_N => true,
                bg_unit_attribute.mAtr_G3_E => true,
                bg_unit_attribute.mAtr_G3_W => true,
                bg_unit_attribute.mAtr_G3_S => true,
                bg_unit_attribute.mAtr_G3_N_ND => false,
                bg_unit_attribute.mAtr_G3_E_ND => false,
                bg_unit_attribute.mAtr_G3_W_ND => false,
                bg_unit_attribute.mAtr_G3_S_ND => false,
                bg_unit_attribute.mAtr_G3_NaW_ND => false,
                bg_unit_attribute.mAtr_G3_SaW_ND => false,
                bg_unit_attribute.mAtr_G3_SaE_ND => false,
                bg_unit_attribute.mAtr_G3_NaE_ND => false,
                bg_unit_attribute.mAtr_G3_NW => false,
                bg_unit_attribute.mAtr_G3_SW => false,
                bg_unit_attribute.mAtr_G3_SE => false,
                bg_unit_attribute.mAtr_G3_NE => false,
                bg_unit_attribute.mAtr_G3_NaW => true,
                bg_unit_attribute.mAtr_G3_SaW => true,
                bg_unit_attribute.mAtr_G3_SaE => true,
                bg_unit_attribute.mAtr_G3_NaE => true,
                bg_unit_attribute.mAtr_SLATE => false,
                _ => true
            };
        }

        private static bool mCoBG_CheckSandHole_ClData(int col)
        {
            if (col < (int)bg_unit_attribute.mAtr_WAVE_SE2)
            {
                if (col != (int)bg_unit_attribute.mAtr_SAND && (col > (int)bg_unit_attribute.mAtr_RIVER_NE || col != (int)bg_unit_attribute.mAtr_SANDHOLE))
                {
                    return false;
                }
            }
            else if (col != (int)bg_unit_attribute.mAtr_WAVE_S)
            {
                if (col > (int)bg_unit_attribute.mAtr_STNB_S)
                {
                    return false;
                }
                if (col > (int)bg_unit_attribute.mAtr_WAVE_SW2)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckCancel12(ushort item)
        {
            if (
                item != 0x0864 &&
                (ushort)(item - 0x0801) > 1 &&
                (ushort)(item - 0x0806) > 1 &&
                (ushort)(item - 0x080E) > 1 &&
                (ushort)(item - 0x0816) > 1 &&
                (ushort)(item - 0x081E) > 1 &&
                (ushort)(item - 0x0826) > 1 &&
                (ushort)(item - 0x082E) > 1 &&
                (ushort)(item - 0x0833) > 1 &&
                (ushort)(item - 0x0838) > 1 &&
                (ushort)(item - 0x0850) > 1 &&
                (ushort)(item - 0x0855) > 1 &&
                (ushort)(item - 0x085E) > 1 &&
                item != 0x0865
            )
            {
                return false;
            }

            return true;
        }

        private static bool CheckCancel32(ushort item)
        {
            if (
                (ushort)(item - 0x0803) > 1 &&
                (ushort)(item - 0x0830) > 1 &&
                (ushort)(item - 0x0835) > 1 &&
                (ushort)(item - 0x083A) > 1 &&
                (ushort)(item - 0x0852) > 1 &&
                (ushort)(item - 0x0860) > 1 &&
                (ushort)(item - 0x0866) > 2 &&
                (ushort)(item - 0x0808) > 4 &&
                (ushort)(item - 0x0810) > 4 &&
                (ushort)(item - 0x0818) > 4 &&
                (ushort)(item - 0x0820) > 4 &&
                (ushort)(item - 0x0828) > 4 &&
                (ushort)(item - 0x0857) > 4 &&
                (ushort)(item - 0x005E) > 3 &&
                item != 0x0069 &&
                (ushort)(item - 0x0078) > 2 &&
                item != 0x0082 &&
                (ushort)(item - 0x007F) > 1 &&
                item != 0x81
            )
            {
                return false;
            }

            return true;
        }

        private static bool CheckCancel22(ushort item)
        {
            if (item != 0x000C)
            {
                if (item < 0x000C)
                {
                    if (item != 0x0007)
                    {
                        return false;
                    }
                }
                else if (item != 0x000E)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckCancel36(ushort item)
        {
            if ((item & 0xF000) == 0x5000 && ((item > 0x580F && item < 0x5825) || (item > 0x4FFF && item < 0x50EF) || item == 0x5829 || item == 0x580C || item == 0x5849))
            {
                return true;
            }
            return false;
        }

        private static bool CheckCancelLeft45(ushort item)
        {
            if ((item & 0xF000) == 0x5000 && (item == 0x5800 || item == 0x5802))
            {
                return true;
            }
            return false;
        }

        private static bool CheckCancelRight45(ushort item)
        {
            if ((item & 0xF000) == 0x5000 && (item == 0x5801 || item == 0x5803))
            {
                return true;
            }
            return false;
        }

        private static bool CheckCancel46(ushort item)
        {
            if (item != 0x5808)
            {
                if (item < 0x5808)
                {
                    if (item != 0x5804)
                    {
                        return false;
                    }
                }
                else if (item != 0x584D)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckCancel23(ushort item)
        {
            return item == 0x5844;
        }

        private static bool CheckCancel57(ushort item)
        {
            return item == 0x5805;
        }

        private static bool CheckCancel68(ushort item)
        {
            if (item != 0x5806 && item != 0x5807)
            {
                return false;
            }

            return true;
        }

        private static bool CheckCancel77(ushort item)
        {
            return item == 0x584A;
        }

        private static void CancelOn(in ushort[][] cancel_ut, int x, int z)
        {
            if (x < 0) return;
            if (x > 0x4F) return; /* [0, 5*16) */
            if (z < 0) return;
            if (z > 0x5F) return; /* [0, 6*16) */

            int b_znum = z / 16;
            int b_xnum = x / 16;

            int ut_x = x & 0xF;
            int ut_z = z & 0xF;

            cancel_ut[b_znum * 5 + b_xnum][ut_z] |= (ushort)(1 << ut_x);
        }

        private static void SetCancelOn(in ushort[][] cancel, int x_base, int z_base, int x_start, int x_end, int z_start, int z_end)
        {
            for (; z_start <= z_end; z_start++)
            {
                for (; x_start <= x_end; x_start++)
                {
                    CancelOn(cancel, x_base + x_start, z_base + z_start);
                }
            }
        }

        private static readonly int[][] l_non_area = new int[11][]
        {
            new int[4]{ 0, 0, -1, 0 },
            new int[4]{ -1, 1, -2, -1 },
            new int[4]{ -1, 0, -1, 0 },
            new int[4]{ -1, 1, -3, 2 },
            new int[4]{ -1, 2, -2, 2 },
            new int[4]{ -2, 1, -2, 2 },
            new int[4]{ -2, 1, -3, 2 },
            new int[4]{ -1, 0, -2, 0 },
            new int[4]{ -3, 1, -4, 2 },
            new int[4]{ -3, 2, -5, 2 },
            new int[4]{ -3, 3, -4, 2 }
        };

        private static readonly Func<ushort, bool>[] check_cancel =
        {
            CheckCancel12,
            CheckCancel32,
            CheckCancel22,
            CheckCancel36,
            CheckCancelLeft45,
            CheckCancelRight45,
            CheckCancel46,
            CheckCancel23,
            CheckCancel57,
            CheckCancel68,
            CheckCancel77
        };

        private static void SetblockCancelUtInfo(in ushort[][] cancel_ut, in ushort[] block, int x_base, int z_base)
        {
            int block_ofs = 0;
            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++, block_ofs++)
                {
                    ushort item = block[block_ofs];
                    for (int i = 0; i < 11; i++)
                    {
                        if (check_cancel[i](item))
                        {
                            SetCancelOn(cancel_ut, x_base + x, z_base + z, l_non_area[i][0], l_non_area[i][1], l_non_area[i][2], l_non_area[i][3]);
                            SetCancelOn(cancel_ut, x_base + x, z_base + z, 0, 0, 0, 0);
                            break;
                        }
                    }
                }
            }
        }

        private static void SetCancelUtInfo(in ushort[][] cancel_ut, in ushort[][] fg)
        {
            for (int z = 0; z < 6; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    SetblockCancelUtInfo(cancel_ut, fg[z * 5 + x], x * 16, z * 16);
                }
            }
        }

        private static void GetDepositAbleFlatNum(ref byte candidate_num, ref byte candidate_flat_num, in ushort[] fg_p, bgdata_item collision, in ushort[] cancel_ut)
        {
            candidate_num = 0;
            candidate_flat_num = 0;

            for (int ut_z = 0; ut_z < 16; ut_z++)
            {
                for (int ut_x = 0; ut_x < 16; ut_x++)
                {
                    if (((cancel_ut[ut_z] >> ut_x) & 1) != 1 && fg_p[ut_z * 16 + ut_x] == 0 && mCoBG_CheckHole_OrgAttr((int)collision.bg_utinfo[ut_z, ut_x] & 0x3F))
                    {
                        candidate_num++;

                        uint ut_info = collision.bg_utinfo[ut_z, ut_x];
                        int center_ofs = (int)(ut_info >> 26) & 0x1F;
                        int rightU_ofs = (int)(ut_info >> 6) & 0x1F;
                        int rightD_ofs = (int)(ut_info >> 11) & 0x1F;
                        int leftD_ofs = (int)(ut_info >> 16) & 0x1F;
                        int leftU_ofs = (int)(ut_info >> 21) & 0x1F;

                        /* All block collisions must be the same */
                        if (center_ofs == leftU_ofs && center_ofs == leftD_ofs && center_ofs == rightD_ofs && center_ofs == rightU_ofs && mCoBG_CheckSandHole_ClData((int)ut_info & 0x3F) != true)
                        {
                            candidate_flat_num++;
                        }
                    }
                }
            }
        }

        private void SetShineGroundBlock(ref int[] shine_pos, in ushort[] fg, in uint[,] col_bg, int flat_num, in ushort[] cancel_ut)
        {
            int selected_ut = (int)(flat_num * fqrand());

            for (int ut_z = 0; ut_z < 16; ut_z++)
            {
                for (int ut_x = 0; ut_x < 16; ut_x++)
                {
                    if (((cancel_ut[ut_z] >> ut_x) & 1) != 1 && fg[ut_z * 16 + ut_x] == 0 && mCoBG_CheckHole_OrgAttr((int)col_bg[ut_z, ut_x] & 0x3F))
                    {
                        uint ut_info = col_bg[ut_z, ut_x];
                        int center_ofs = (int)(ut_info >> 26) & 0x1F;
                        int rightU_ofs = (int)(ut_info >> 6) & 0x1F;
                        int rightD_ofs = (int)(ut_info >> 11) & 0x1F;
                        int leftD_ofs = (int)(ut_info >> 16) & 0x1F;
                        int leftU_ofs = (int)(ut_info >> 21) & 0x1F;

                        /* All block collisions must be the same */
                        if (center_ofs == leftU_ofs && center_ofs == leftD_ofs && center_ofs == rightD_ofs && center_ofs == rightU_ofs && mCoBG_CheckSandHole_ClData((int)ut_info & 0x3F) != true)
                        {
                            if (selected_ut == 0)
                            {
                                shine_pos[2] = ut_x;
                                shine_pos[3] = ut_z;
                                cancel_ut[ut_z] |= (ushort)(1 << ut_x); // Mark unit as occupied
                                return;
                            }
                            selected_ut--;
                        }
                    }
                }
            }
        }

        private void SetShineGround(ref int[] shine_pos, ref int block_flat_candidates, in byte[] candidate_flat_num, ref int block_candidates, in byte[] candidate_num, in ushort[][] cancel_ut, in ushort[][] fg, in ushort[] bg, in bgdata_item[] bg_data)
        {
            if (block_flat_candidates != 0)
            {
                int selected_block = (int)(block_flat_candidates * fqrand());
                for (int block = 0; block < 30; block++)
                {
                    if (candidate_flat_num[block] != 0)
                    {
                        if (selected_block == 0)
                        {
                            int b_xnum = block % 5;
                            int b_znum = block / 5;
                            int combi_ofs = (b_znum + 1) * 7 + (b_xnum + 1);

                            shine_pos[0] = b_xnum + 1;
                            shine_pos[1] = b_znum + 1;
                            uint[,] col_bg = bg_data[data_combi_table[bg[combi_ofs] / 4].bg_name].bg_utinfo;
                            SetShineGroundBlock(ref shine_pos, fg[block], col_bg, candidate_flat_num[block], cancel_ut[block]);
                            candidate_flat_num[block]--;
                            if (candidate_flat_num[block] == 0)
                            {
                                block_flat_candidates--;
                            }
                            candidate_num[block]--;
                            if (candidate_num[block] == 0)
                            {
                                block_candidates--;
                            }
                            return;
                        }

                        selected_block--;
                    }
                }
            }
        }

        private void SetDigItem(in ushort[] save_combi_table, in ushort[][] fg, in ushort[][] cancel_ut, out int[] shine_pos)
        {
            byte[] candidate_num = new byte[30];
            byte[] candidate_flat_num = new byte[30];
            int block_candidates = 0;
            int block_flat_candidates = 0;

            for (int b_znum = 0; b_znum < 6; b_znum++)
            {
                for (int b_xnum = 0; b_xnum < 5; b_xnum++)
                {
                    int ofs = b_znum * 5 + b_xnum;
                    int combi_ofs = (b_znum + 1) * 7 + (b_xnum + 1);

                    if (CheckBlockKind_OR(save_combi_table[combi_ofs] >> 2, (int)block_info.PLACE_PLAYER_HOUSE | (int)block_info.PLACE_SQUARE | (int)block_info.PLACE_STATION | (int)block_info.PLACE_POOL | (int)block_info.PLACE_DUMP) == false)
                    {
                        GetDepositAbleFlatNum(ref candidate_num[ofs], ref candidate_flat_num[ofs], fg[ofs], bg_items[data_combi_table[save_combi_table[combi_ofs] >> 2].bg_name], cancel_ut[ofs]);

                        if (candidate_num[ofs] != 0)
                        {
                            block_candidates++;
                        }

                        if (candidate_flat_num[ofs] != 0)
                        {
                            block_flat_candidates++;
                        }
                    }
                }
            }

            int[][] shine_positions = new int[5][] { new int[4], new int[4], new int[4], new int[4], new int[4] };

            for (int i = 0; i < 5; i++)
            {
                SetShineGround(ref shine_positions[i], ref block_flat_candidates, candidate_flat_num, ref block_candidates, candidate_num, cancel_ut, fg, save_combi_table, bg_items);
            }

            shine_pos = shine_positions[0];

            // Gyroid emulation
            ushort[] haniwas = new ushort[3];
            for (int i = 0; i < 3; i++)
            {
                // emulate mSP_RandomHaniwaSelect
                bool set = false;
                while (!set)
                {
                    set = true;
                    ushort haniwa = (ushort)((fqrand() * 127.0f) * 4 + 0x15B0);
                    for (int x = 0; x < i; x++)
                    {
                        if (haniwas[x] == haniwa)
                        {
                            set = false;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                fqrand_raw(); // Select candidate block (mAGrw_SetHaniwa)
                fqrand_raw(); // Select candidate unit (mMsm_DepositItemBlock_cancel)
            }
        }

        private static int CountNormalStone(in ushort[] fg_block)
        {
            int count = 0;

            foreach (ushort item in fg_block)
            {
                if (item > 0x62 && item < 0x68)
                {
                    count++;
                }
            }

            return count;
        }

        private void SetMoneyStoneBlock(ref int[] rock_pos, in ushort[] fg, int candidate_num)
        {
            int selected_ut = (int)(candidate_num * fqrand());
            for (int i = 0; i < 16 * 16; i++)
            {
                if (fg[i] > 0x62 && fg[i] < 0x68)
                {
                    if (selected_ut == 0)
                    {
                        rock_pos[2] = i % 16;
                        rock_pos[3] = i / 16;
                        return;
                    }

                    selected_ut--;
                }
            }
        }

        private void SetMoneyStone(ref int[] rock_pos, in ushort[][] fg)
        {
            /* Note: this actually generates 5 positions, but we only care about the first one */
            byte[] candidate_num = new byte[30];
            int candidate_blocks = 0;
            int total_rocks = 0;

            // Count all normal rocks in each acre
            for (int i = 0; i < 30; i++)
            {
                int count = CountNormalStone(fg[i]);
                candidate_num[i] = (byte)count;

                if (count != 0)
                {
                    total_rocks += count;
                    candidate_blocks++;
                }
            }

            if (total_rocks > 0)
            {
                int selected_block = (int)(candidate_blocks * fqrand());
                for (int i = 0; i < 30; i++)
                {
                    if (candidate_num[i] != 0)
                    {
                        if (selected_block == 0)
                        {
                            rock_pos[0] = 1 + (i % 5);
                            rock_pos[1] = 1 + (i / 5);
                            SetMoneyStoneBlock(ref rock_pos, fg[i], candidate_num[i]);
                            return;
                        }

                        selected_block--;
                    }
                }
            }
        }

        private int mMsm_GetDepositAbleNum_cancel(in ushort[] fg, bgdata_item bg, in ushort[] cancel_ut)
        {
            int num = 0;
            for (int ut_z = 0; ut_z < 16; ut_z++)
            {
                for (int ut_x = 0; ut_x < 16; ut_x++)
                {
                    if (((cancel_ut[ut_z] >> ut_x) & 1) != 1 && fg[ut_z * 16 + ut_x] == 0 && mCoBG_CheckHole_OrgAttr((int)bg.bg_utinfo[ut_z, ut_x] & 0x3F))
                    {
                        num++;
                    }
                }
            }

            return num;
        }

        private void mMsm_DepositItemBlock_cancel(in ushort[] fg, ushort item, bgdata_item col, in ushort[] deposit, in ushort[] cancel_ut, int num_depositable)
        {
            int deposit_ut = (int)(fqrand() * num_depositable);

            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (((cancel_ut[z] >> x) & 1) != 1 && fg[z * 16 + x] == 0 && mCoBG_CheckHole_OrgAttr((int)col.bg_utinfo[z, x] & 0x3F))
                    {
                        if (deposit_ut == 0)
                        {
                            fg[z * 16 + x] = item;
                            deposit[z] |= (ushort)(1 << x);
                            return;
                        }
                    }
                }
            }
        }

        private void mMsm_DepositItemBlock_cancelMOD(in ushort[] fg, ushort item, bgdata_item col, in ushort[] cancel_ut, int num_depositable)
        {
            int deposit_ut = (int)(fqrand() * num_depositable);

            for (int z = 0; z < 16; z++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (((cancel_ut[z] >> x) & 1) != 1 && fg[z * 16 + x] == 0 && mCoBG_CheckHole_OrgAttr((int)col.bg_utinfo[z, x] & 0x3F))
                    {
                        if (deposit_ut == 0)
                        {
                            fg[z * 16 + x] = item;
                            return;
                        }
                    }
                }
            }
        }

        private void mMsm_DepositFossilBlock(in ushort[] fg, bgdata_item col, in ushort[] deposit, in ushort[] cancel_ut, int num_depositable)
        {
            mMsm_DepositItemBlock_cancel(fg, 0x2511, col, deposit, cancel_ut, num_depositable);
        }

        private void mMsm_DepositFossilBlockMOD(in ushort[] fg, bgdata_item col, in ushort[] cancel_ut, int num_depositable)
        {
            mMsm_DepositItemBlock_cancelMOD(fg, 0x2511, col, cancel_ut, num_depositable);
        }

        private void mMsm_DepositFossilBlockLine(int b_xnum, in ushort[] save_combi_table, in ushort[][] fg, in ushort[][] cancel_ut)
        {
            int[] depositable_line = new int[6];
            int depositable_blocks = 0;

            for (int b_znum = 0; b_znum < 6; b_znum++)
            {
                int combi_ofs = (b_znum + 1) * 7 + (b_xnum + 1);
                int combi = save_combi_table[combi_ofs] >> 2;
                if (!CheckBlockKind_OR(combi, (int)block_info.PLACE_PLAYER_HOUSE | (int)block_info.PLACE_SQUARE | (int)block_info.PLACE_STATION | (int)block_info.PLACE_POOL | (int)block_info.PLACE_DUMP))
                {
                    int count = mMsm_GetDepositAbleNum_cancel(fg[b_znum * 5 + b_xnum], bg_items[data_combi_table[combi].bg_name], cancel_ut[b_znum * 5 + b_xnum]);
                    depositable_line[b_znum] = count;
                    if (count != 0)
                    {
                        depositable_blocks++;
                    }
                }
            }

            if (depositable_blocks > 0)
            {
                int selected_block = (int)(fqrand() * depositable_blocks);

                for (int b_znum = 0; b_znum < 6; b_znum++)
                {
                    if (depositable_line[b_znum] != 0)
                    {
                        if (selected_block == 0)
                        {
                            int combi_ofs = (b_znum + 1) * 7 + (b_xnum + 1);
                            bgdata_item col = bg_items[data_combi_table[save_combi_table[combi_ofs] >> 2].bg_name];
                            mMsm_DepositFossilBlockMOD(fg[b_znum * 5 + b_xnum], col, cancel_ut[b_znum * 5 + b_xnum], depositable_line[b_znum]);
                            return;
                        }
                        selected_block--;
                    }
                }
            }
        }

        private void mMsm_DepositFossil(in ushort[] save_combi_table, in ushort[][] fg, in ushort[][] cancel_ut)
        {
            int deposit_record = 0; // rows (y-acres) already containing a fossil
            for (int i = 5; i != 0; i--)
            {
                int selected_block = (int)(fqrand() * i);
                for (int b_xnum = 0; b_xnum < 5; b_xnum++)
                {
                    // Check no fossil is already in this acre row.
                    if ((deposit_record >> (b_xnum + 1) & 1) == 0)
                    {
                        if (selected_block < 1)
                        {
                            mMsm_DepositFossilBlockLine(b_xnum, save_combi_table, fg, cancel_ut);
                            deposit_record |= 1 << (b_xnum + 1);
                            break;
                        }
                        selected_block--;
                    }
                }
            }
        }

        /* emulates mAGrw_RenewalFgItem_ovl */
        public void GetShineSpotAndMoneyStonePositions(in ushort[] save_combi_table, in ushort[][] fg, out int[][] locs)
        {
            ushort[][] cancel_ut = new ushort[30][];
            for (int i = 0; i < 30; i++)
            {
                cancel_ut[i] = new ushort[16]; // each bit is one unit tile
            }

            SetCancelUtInfo(cancel_ut, fg);
            mMsm_DepositFossil(save_combi_table, fg, cancel_ut);
            SetDigItem(save_combi_table, fg, cancel_ut, out int[] shine_pos);
            int[] rock_pos = new int[4]; // [0] = b_xnum, [1] = b_znum, [2] = ut_xnum, [3] = ut_znum
            SetMoneyStone(ref rock_pos, fg);
            locs = new int[2][] { shine_pos, rock_pos };
        }

        public int[,] GetMapIcons(in ushort[] save_combi_table)
        {
            int[,] icons = new int[6,5];

            for (int b_znum = 1; b_znum < 7; b_znum++)
            {
                for (int b_xnum = 1; b_xnum < 6; b_xnum++)
                {
                    int combi_ofs = b_znum * 7 + b_xnum;
                    icons[b_znum - 1, b_xnum - 1] = data_combi_table[save_combi_table[combi_ofs] >> 2].type;
                }
            }

            return icons;
        }

        public static void DEBUG_PrintAllDataCombiTypeInfo()
        {
            for (int i = 0; i < 32; i++)
            {
                Console.WriteLine($"{(1 << i):X} - {(block_info)(1 << i)}");
            }

            for (int i1 = 0; i1 < data_combi_table.Length; i1++)
            {
                data_combi combi = data_combi_table[i1];
                int type = combi.type < 0x6C ? mRF_block_info[combi.type] : 0;
                Console.Write($"Acre: {(i1 << 2):X4} - ");
                if (type == 0)
                {
                    Console.WriteLine("PLACE_NONE");
                }
                else
                {
                    int w = 0;
                    for (int i = 0; i < 32; i++)
                    {
                        if ((type & (1 << i)) != 0)
                        {
                            if (w != 0)
                            {
                                Console.Write(" | ");
                            }
                            w++;
                            Console.Write((block_info)(type & (1 << i)));
                        }
                    }
                    Console.Write("\n");
                }
            }
        }

        public static void DEBUG_TestSeedSearchTEST()
        {
            TownGeneration gen = new(0);
            Stopwatch w = Stopwatch.StartNew();
            const uint desired_seed = 0x671D11F8;
            int count = 0;

            for (int i = 0; i < 1_000_000; i++)
            {
                /* Minimum calls is 27 */
                for (int x = 0; x < 27; x++)
                {
                    gen.fqrand_raw();
                }

                for (int x = 27; x < 5_000; x++)
                {
                    if (gen.fqrand_raw() == desired_seed)
                    {
                        count++;
                        break;
                    }
                }
            }

            w.Stop();
            Console.WriteLine($"1,000,000x5,000 calls took: {(double)w.ElapsedTicks / TimeSpan.TicksPerMillisecond} ms");
            Console.WriteLine($"Found {count} potential seeds");
        }
    }
}
