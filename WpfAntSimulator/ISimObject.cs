using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;

namespace WpfAntSimulator
{
    public interface ISimObject
    {
        Point Position { get; set; } // Each object has a position.
        Color MyColor { get; set; } // Each object has a color.
        void Update(Bitmap bm); // This function updates the object's position etc.
        void Render(Bitmap bm); // This function renders the object.
        void Enlarge(Bitmap bm); // This function enlarges the object (only for single pixel objects such as ants).
        bool ShouldBeRendered(); // This function is for determining if the object should be rendered and if not then removed all together.
    }
}
