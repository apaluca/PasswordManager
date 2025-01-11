using PasswordManager.Data.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories.Interfaces
{
        public interface IAuditLogRepository
        {
                void LogAction(int? userId, string action, string details, string ipAddress);
                IEnumerable<AuditLog> GetUserLogs(int userId);
                IEnumerable<AuditLog> GetRecentLogs(int count);
                IEnumerable<AuditLog> GetLogsByDateRange(DateTime start, DateTime end);
        }
}
