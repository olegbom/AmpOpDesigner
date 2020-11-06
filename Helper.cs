using System;
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

        public static double RoundUp(double n)
        {
            double power = Math.Truncate(Math.Log10(n));
            double nbase = n / Math.Pow(10, power);
            for (int i = 1; i < E24.Count; i++)
            {
                if (nbase < E24[i])
                    return E24[i] * Math.Pow(10, power);

            }
            return Math.Pow(10, power + 1);
        }

        public static double RoundDown(double n)
        {
            double power = Math.Truncate(Math.Log10(n));
            double nbase = n / Math.Pow(10, power);
            for (int i = 0; i < E24.Count; i++)
            {
                if (nbase > E24[i])
                    return E24[i] * Math.Pow(10, power);
            }
            return Math.Pow(10, power);
        }

        public static double Round(double n)
        {
            var up = RoundUp(n);
            var down = RoundDown(n);
            return Math.Abs(up - n) < Math.Abs(down - n) ? up : down;
        }
    }
}