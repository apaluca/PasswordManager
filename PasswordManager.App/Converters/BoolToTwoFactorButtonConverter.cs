using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PasswordManager.App.Converters
{
        public class BoolToTwoFactorButtonConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                {
                        return (bool)value ? "Disable 2FA" : "Enable 2FA";
                }

                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }
}
