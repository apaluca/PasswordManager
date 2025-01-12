using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Models
{
        public class SecurityIncidentModel
        {
                public DateTime Timestamp { get; set; }
                public string Username { get; set; }
                public string IPAddress { get; set; }
                public string Details { get; set; }
                public string Severity { get; set; } = "Medium";
        }
}
