using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAntSimulator.SimObjects.Pheremone
{
    public class RedTrail : ISimObject
    {
        public Point Position { get; set; }
        public Color MyColor { get; set; }
        private Color originalColor;
        private int initialScentValue;

        public int ScentValue { get; set; } // degrades by 1 after each tick

        public RedTrail(Point p)
        {
            Position = new Point(p.X, p.Y);
            MyColor = Globals.redTrailColor;
            originalColor = Globals.redTrailColor;
            initialScentValue = 200;
            ScentValue = initialScentValue;
        }

        public void Enlarge(Bitmap bm)
        {
            Globals.ColorPixel(Position.X - 1, Position.Y, MyColor, bm);
            Globals.ColorPixel(Position.X + 1, Position.Y, MyColor, bm);
            Globals.ColorPixel(Position.X, Position.Y + 1, MyColor, bm);
            Globals.ColorPixel(Position.X, Position.Y - 1, MyColor, bm);
        }

        public void Render(Bitmap bm)
        {
            bm.SetPixel(Position.X, Position.Y, MyColor);
            Enlarge(bm);
        }

        public void Update(Bitmap bm)
        {
            ScentValue--;
            Color newColor = Color.FromArgb((int)(originalColor.A * Clamp((double)ScentValue / initialScentValue)),
                                            MyColor.R, MyColor.G, MyColor.B);
            MyColor = newColor;
        }
        public bool ShouldBeRendered()
        {
            if(ScentValue > 0) return true;
            return false;
        }

        public void AddScent()
        {
            ScentValue += 200;
        }

        private double Clamp(double i)
        {
            if (i > 1) return 1;
            return i;
        }
    }
}
