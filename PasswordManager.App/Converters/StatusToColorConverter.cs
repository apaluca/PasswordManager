using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PasswordManager.App.Converters
{
        public class StatusToColorConverter : IValueConverter
        {
                public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
                {
                        string status = value as string;
                        if (status != null)
                        {
                                switch (status.ToLower())
                                {
                                        case "good":
                                                return "#E8F5E9"; // Light green
                                        case "warning":
                                                return "#FFF3E0"; // Light orange
                                        case "critical":
                                                return "#FFEBEE"; // Light red
                                        case "info":
                                                return "#E3F2FD"; // Light blue
                                        default:
                                                return "#F5F5F5"; // Light gray
                                }
                        }

                        return "#F5F5F5"; // Light gray as a fallback for null or non-string values
                }

                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                {
                        throw new NotImplementedException();
                }
        }
}
