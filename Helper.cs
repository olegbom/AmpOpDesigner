using System.Collections.Generic;
using System.Windows;

namespace AmpOpDesigner
{
    static class Helper
    {
        public static Point ToPoint(this Vector v) => new Point(v.X + 0.5, v.Y + 0.5);

        public static IReadOnlyList<double> E24 { get; private set; } = new List<double>()
        {
            1  , 1.1, 1.2, 1.3, 1.5, 1.6, 
            1.8, 2  , 2.2, 2.4, 2.7, 3  ,
            3.3, 3.6, 3.9, 4.3, 4.7, 5.1,
            5.6, 6.2, 6.8, 7.5, 8.2, 9.1
        };
    }
}