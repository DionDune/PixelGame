using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGame
{
    internal class Tile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Tag { get; set; }

        public byte Type { get; set; }
        public byte TextureTag { get; set; }

        public byte WallType { get; set; }
        public byte WallTextureTag { get; set; }

        public sbyte LightLevel { get; set; }
    }
}
