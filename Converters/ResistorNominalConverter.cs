using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AmpOpDesigner.Converters
{
    class ResistorNominalConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double r)
            {
                r = Math.Round(r);
                if (r < 1000)
                {
                    return $"{r:F0}";
                }
                else if (r < 1000_000)
                {
                    double kOhm = Math.Truncate(r / 1000);
                    double ohm = r - kOhm * 1000;

                    string strOhm = ((ohm < 1) ? "" : $"{Math.Round(ohm):000}");
                    strOhm = strOhm.TrimEnd('0');

                    return $"{kOhm:F0}K{strOhm}";
                }
                else
                {
                    double MOhm = Math.Truncate(r / 1000_000);
                    double kOhm = Math.Round((r - MOhm*1000_000) / 1000);
                    string strKOhm = $"{kOhm:000}";
                    strKOhm = strKOhm.TrimEnd('0');
                    return $"{MOhm:F0}M{strKOhm}";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
