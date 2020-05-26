using System;

namespace GBAEmulator
{
    partial class PPU
    {
        ushort[][] OBJLayers = new ushort[4][] { new ushort[width], new ushort[width], new ushort[width], new ushort[width] };

        private void ResetOBJScanlines(params byte[] Priorities)
        {
            foreach (byte Priority in Priorities)
            {
                for (byte x = 0; x < width; x++)
                {
                    OBJLayers[Priority][x] = 0;
                }
            }
        }

        struct OBJSize
        {
            public readonly byte Width, Height;
            public OBJSize(byte Width, byte Height)
            {
                this.Width = Width;
                this.Height = Height;
            }
        }

        // [Shape][Size]
        private static readonly OBJSize[][] GetOBJSize = new OBJSize[3][]
        {
            new OBJSize[4] {new OBJSize(8, 8), new OBJSize(16, 16), new OBJSize(32, 32), new OBJSize(32, 32) },
            new OBJSize[4] {new OBJSize(16, 8), new OBJSize(32, 8), new OBJSize(32, 16), new OBJSize(64, 32) },
            new OBJSize[4] {new OBJSize(8, 16), new OBJSize(8, 32), new OBJSize(16, 32), new OBJSize(32, 64) }
        };

    }
}
