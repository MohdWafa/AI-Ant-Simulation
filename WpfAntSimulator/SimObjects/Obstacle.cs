using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;
using System.Windows;

namespace WpfAntSimulator.SimObjects
{
    class Obstacle : ISimObject
    {
        public Point Position { get; set; }
        public Color MyColor { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public bool IsInBound(int x, int y, Bitmap bm)
        {
            if (x > 0 && x < bm.Width && y > 0 && y < bm.Height)
            {
                return true;
            }
            return false;
        }

        public Obstacle()
        {
            MyColor = Color.Brown;
        }

        public void Update(Bitmap bm)
        {
            
        }
        public void Render(Bitmap bm)
        {
            for (int x = Convert.ToInt32(Position.X - (Width / 2)); x < Convert.ToInt32(Position.X + (Width / 2)); x++)
            {
                for (int y = Convert.ToInt32(Position.Y - (Height / 2)); y < Convert.ToInt32(Position.Y + (Height / 2)); y++)
                    if (IsInBound(x, y, bm))
                        bm.SetPixel(x, y, MyColor);
            }            
        }
        public void Enlarge(Bitmap bm)
        {

        }
        public bool ShouldBeRendered()
        {
            return true;
        }
    }
}
