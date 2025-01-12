using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace PasswordManager.App.Converters
{
        public class BooleanToVisibilityConverter : IValueConverter
        {
                public bool Inverse { get; set; }
                public bool UseHidden { get; set; }

                public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                {
                        bool bValue = value is bool boolean ? boolean : false;

                        if (Inverse)
                                bValue = !bValue;

                        if (bValue)
                                return Visibility.Visible;
                        else
                                return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
                }

                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                {
                        if (value is Visibility visibility)
                        {
                                bool result = visibility == Visibility.Visible;
                                return Inverse ? !result : result;
                        }

                        return false;
                }
        }
}
