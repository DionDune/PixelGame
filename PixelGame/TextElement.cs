using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGame
{
    internal class TextElement
    {
        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public string Text { get; set; }

        public List<List<bool>> Elements { get; set; }
        public int ElementSize { get; set; }
        
        public Color Color { get; set; }
        public Color BackgroundColor { get; set; }

        public bool hasBackground { get; set; }

        public TextElement()
        {
            XOffset = 0;
            YOffset = 0;

            Text = "EXAMPLE";
            Elements = TextCharacter.GetString(Text);
            ElementSize = 5;

            Color = Color.Black;
            BackgroundColor = Color.White;

            hasBackground = false;
        }

    }
}
