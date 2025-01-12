using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Models
{
        public class LoginAttemptModel
        {
                public int AttemptId { get; set; }
                public string Username { get; set; }
                public DateTime? AttemptDate { get; set; }
                public bool IsSuccessful { get; set; }
                public string IPAddress { get; set; }
                public string UserAgent { get; set; }
        }
}
