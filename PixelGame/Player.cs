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

        public int Health { get; set; }
        public int Health_Max { get; set; }
        public int HealthRedgainAmount { get; set; }
        public int HealthRegainInterval { get; set; }

        public int Breath { get; set; }
        public int Breath_Max { get; set; }
        public int BreathRegainAmount { get; set; }
        public int BreathRegainInterval { get; set; }

        

        public int Momentum_Vertical { get; set; }
        public int Momentum_Horizontal { get; set; }

        public float Speed_Crouch { get; set; }
        public float Speed_Base { get; set; }
        public float Speed_Shift { get; set; }

        public int JumpHeight_Crouch { get; set; }
        public int JumpHeight { get; set; }
        public int JumpHeight_Shift { get; set; }

        public bool IsShifting { get; set; }
        public bool IsCrouching { get; set; }

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

        public void RegainHandler(uint Tick)
        {
            //Health
            if (Tick % HealthRegainInterval == 0)
            {
                Health += HealthRedgainAmount;
                if (Health > Health_Max)
                {
                    Health = Health_Max;
                }
            }
            //Breath
            if (Tick % BreathRegainInterval == 0)
            {
                Breath += BreathRegainAmount;
                if (Breath > Breath_Max)
                {
                    Breath = Breath_Max;
                }
            }
        }
    }
}
