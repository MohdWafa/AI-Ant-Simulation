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
    class Colony : ISimObject
    {
        public Point Position { get; set; }
        public Color MyColor { get; set; }

        public int Radius { get; set; }


        public Colony()
        {
            MyColor = Color.Violet;
        }
        public Colony(Point p)
        {
            Position = p;
            MyColor = Color.Violet;
        }

        public Colony(Point p, int radius)
        {
            Position = p;
            Radius = radius;
            MyColor = Color.Violet;
        }

        private Bitmap midPointCircleDraw(int x_centre, int y_centre, int r, Bitmap bm)
        {
            int x = r, y = 0;

            if (r > 0)
            {
                ColorPixel(x + x_centre, -y + y_centre, bm);
                ColorPixel(y + x_centre, x + y_centre, bm);
                ColorPixel(-y + x_centre, x + y_centre, bm);
            }

            int P = 1 - r;
            while (x > y)
            {
                y++;
                if (P <= 0) P = P + 2 * y + 1;
                else
                {
                    x--;
                    P = P + 2 * y - 2 * x + 1;
                }

                if (x < y) break;
                ColorPixel(x + x_centre, y + y_centre, bm); // Top right
                ColorPixel(-x + x_centre, y + y_centre, bm); // Top left
                ColorPixel(x + x_centre, -y + y_centre, bm); // Bottom right
                ColorPixel(-x + x_centre, -y + y_centre, bm); // Bottom left

                if (x != y)
                {
                    ColorPixel(y + x_centre, x + y_centre, bm); // Top right
                    ColorPixel(-y + x_centre, x + y_centre, bm); // Top left
                    ColorPixel(y + x_centre, -x + y_centre, bm); // Bottom right
                    ColorPixel(-y + x_centre, -x + y_centre, bm); // Bottom left
                }
            }

            ColorPixel(x, y - Radius - 1, bm);
            ColorPixel(x - Radius, y + 1, bm);
            return bm;
        }
        
        public void Update(Bitmap bm)
        {

        }

        public bool IsInCircle(Point p)
        {
            if (CalcEucliDist(p, Position) <= Radius) return true;
            return false;
        }

        private double CalcEucliDist(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2));
        }
        private double D(int r, int y)
        {
            return Math.Ceiling(Math.Sqrt(r * r - y * y)) - Math.Sqrt(r * r - y * y);
        }
        public void ColorPixel(int x, int y, Color c, Bitmap bm)
        {
            if (!IsInBound(x, y, bm)) return;
            bm.SetPixel(x, y, c);
        }
        public void ColorPixel(int x, int y, Bitmap bm)
        {
            if (!IsInBound(x, y, bm)) return;
            bm.SetPixel(x, y, MyColor);
        }
        
        public bool IsInBound(int x, int y, Bitmap bm)
        {
            if (x > 0 && x < bm.Width && y > 0 && y < bm.Height)
            {
                return true;
            }
            return false;
        }

        public void Render(Bitmap bm)
        {
            // This should render a circle and the Position should be its center. For now we can just have it be a 1 pixel point.
            // There also should be a function here that can check whether an ant is on the circle or within it.
            Point EdgePoint = IsInBound(Position.X + Radius, Position.Y, bm) ?
                new Point(Position.X + Radius, Position.Y) : new Point(Position.X - Radius, Position.Y);

            int r = (int)CalcEucliDist(Position, EdgePoint);

            
            bm = midPointCircleDraw(Position.X, Position.Y, r, bm);


            // This is temporary
            //bm.SetPixel(Position.X, Position.Y, MyColor);
            //Enlarge(bm);
        }


        // This function is to make 1 pixel objects look a bit bigger then they actually are.
        public void Enlarge(Bitmap bm)
        {
            bm.SetPixel(Position.X - 1, Position.Y, MyColor);
            bm.SetPixel(Position.X + 1, Position.Y, MyColor);
            bm.SetPixel(Position.X, Position.Y + 1, MyColor);
            bm.SetPixel(Position.X, Position.Y - 1, MyColor);
        }
        public bool ShouldBeRendered()
        {
            return true;
        }
    }
}
