using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.App.ViewModels
{
        public class SecurityMetric
        {
                public string Name { get; set; }
                public string Value { get; set; }
                public string Status { get; set; } // "Good", "Warning", "Critical", "Info"
        }
}
