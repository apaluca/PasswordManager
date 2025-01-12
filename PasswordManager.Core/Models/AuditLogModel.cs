using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Models
{
        public class AuditLogModel
        {
                public int LogId { get; set; }
                public int? UserId { get; set; }
                public DateTime? ActionDate { get; set; }
                public string Action { get; set; }
                public string Details { get; set; }
                public string IPAddress { get; set; }
                public string Username { get; set; }
        }
}
