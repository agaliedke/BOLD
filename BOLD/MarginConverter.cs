using System.Windows;
using System.Windows.Data;

namespace BOLD
{
    class MarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[1]==null || (double)values[1] == -1 )
                return new Thickness(0, 0, 0, 0);
            else
                return new Thickness(0, 0, 0, System.Convert.ToDouble((double)values[0]*(double)values[1]-12));
        }

        public object[] ConvertBack(object value, System.Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
