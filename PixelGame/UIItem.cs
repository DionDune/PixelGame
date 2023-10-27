﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGame
{
    internal class UIItem
    {
        public string Type { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public int CentreX { get; set; }
        public int CentreY { get; set; }

        public int BorderWidth { get; set; }

        public Color BaseColor { get; set; }
        public Color BorderColor { get; set; }
        public Color HighlightedColor { get; set; }

        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int Value { get; set; }
        
        public string Target { get; set; }
        public int FillDirection { get; set; }

        TextElement Text { get; set; }

        public UIItem()
        {
            Type = "Button";

            X = 0;
            Y = 0;

            Width = 10;
            Height = 10;

            CentreX = 5;
            CentreY = 5;

            BorderWidth = 0;

            BaseColor = Color.Purple;
            BorderColor = Color.Black;
            HighlightedColor = Color.Gold;

            MinValue = 0;
            MaxValue = 1;
            Value = 0;
            FillDirection = 0;

            Target = string.Empty;


            Text = null;
        }
    }
}
