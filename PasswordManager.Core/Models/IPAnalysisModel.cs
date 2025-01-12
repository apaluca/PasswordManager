using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Models
{
        public class IPAnalysisModel
        {
                public string IPAddress { get; set; }
                public int TotalAttempts { get; set; }
                public int FailedAttempts { get; set; }
                public DateTime? LastAttempt { get; set; }
                public double SuccessRate { get; set; }
                public string RiskLevel { get; set; }
        }
}
