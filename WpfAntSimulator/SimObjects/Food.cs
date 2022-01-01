using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;

namespace WpfAntSimulator.SimObjects
{
    public class Food : ISimObject
    {
        public int FoodAmount { get; set; }


        public Point Position { get; set; }
        public Color MyColor { get; set; }

        public Food()
        {
            MyColor = Globals.foodColor;
        }
        public Food(Point point)
        {
            Position = point;
            MyColor = Globals.foodColor;
            FoodAmount = 1;
        }

        public void Update(Bitmap bm)
        {

        }
        public void Render(Bitmap bm)
        {
            bm.SetPixel(Position.X, Position.Y, MyColor);
        }

        public void Enlarge(Bitmap bm)
        {
            bm.SetPixel(Position.X - 1, Position.Y, MyColor);
            bm.SetPixel(Position.X + 1, Position.Y, MyColor);
            bm.SetPixel(Position.X, Position.Y + 1, MyColor);
            bm.SetPixel(Position.X, Position.Y - 1, MyColor);
        }
        public bool ShouldBeRendered()
        {
            if (FoodAmount > 0) return true;
            return false;
        }
    }
}
