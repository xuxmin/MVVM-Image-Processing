using System;
using System.Windows.Data;

namespace MVVM_Image_Processing
{
    public class ValueConverter:IValueConverter
    {
        public object Convert(object value,Type targetType,object parameter,System.Globalization.CultureInfo culture)
        {
            if (!(bool)value)
            {
                return "hidden";
            }
            else
            {
                return "visible";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if((string)value == "hidden")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
