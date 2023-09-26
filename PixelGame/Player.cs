using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGame
{
    internal class Player
    {
        public int x { get; set; }
        public int y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public uint Health { get; set; }
        public uint Health_Max { get; set; }

        public uint Breath { get; set; }
        public uint Breath_Max { get; set; }

        public int JumpHeight { get; set; }

        public int Momentum_Vertical { get; set; }
        public int Momentum_Horizontal { get; set; }

        public float Speed_Base { get; set; }
        public float Speed_Max { get; set; }

        public bool IsFlying { get; set; }

        public bool IsMovingLeft { get; set; }
        public bool IsMovingRight { get; set; }
        public bool IsFlyingUp { get; set; }
        public bool IsFlyingDown { get; set; }
        public bool IsJumping { get; set; }

        public Player()
        {
            x = 0;
            y = 0;

            IsFlying = false;
        }
    }
}
