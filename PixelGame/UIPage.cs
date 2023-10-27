using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelGame
{
    internal class UIPage
    {
        public string Type { get; set; }

        public List<UIItem> UIItems { get; set; }

        public UIPage()
        {
            Type = "Default";

            UIItems = new List<UIItem>();
        }
    }
}
