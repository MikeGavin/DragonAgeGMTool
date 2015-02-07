using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Scrivener.Helpers
{
    //https://code.msdn.microsoft.com/windowsdesktop/Grouping-Expanders-just-b1bbba57
    public class ExpandedConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((string)value == (string)parameter);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return parameter;
        }
    }
}
