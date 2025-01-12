using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories
{
        public class AuditLogRepository : IAuditLogRepository
        {
                private readonly PasswordManagerDataContext _context;

                public AuditLogRepository(PasswordManagerDataContext context)
                {
                        _context = context;
                }

                public void LogAction(int? userId, string action, string details, string ipAddress)
                {
                        var log = new AuditLog
                        {
                                UserId = userId,
                                Action = action,
                                Details = details,
                                IPAddress = ipAddress,
                                ActionDate = DateTime.Now
                        };

                        _context.AuditLogs.InsertOnSubmit(log);
                        _context.SubmitChanges();
                }

                public IEnumerable<AuditLog> GetSystemActivityLogs(int count)
                {
                        return _context.AuditLogs
                            .Where(l =>
                                l.Action.Contains("User") ||      // User management actions
                                l.Action.Contains("Role") ||      // Role changes
                                l.Action.Contains("Password") ||  // Password resets
                                l.Action.Contains("2FA"))         // 2FA changes
                            .OrderByDescending(l => l.ActionDate)
                            .Take(count);
                }

                public IEnumerable<AuditLog> GetSecurityActivityLogs(int count)
                {
                        return _context.AuditLogs
                            .Where(l =>
                                l.Action.Contains("Login") ||     // Login attempts
                                l.Action.Contains("Failed") ||    // Failed actions
                                l.Action.Contains("Access") ||    // Access attempts
                                l.Action.Contains("Security"))    // Security changes
                            .OrderByDescending(l => l.ActionDate)
                            .Take(count);
                }

                public IEnumerable<AuditLog> GetUserLogs(int userId)
                {
                        return _context.AuditLogs
                            .Where(l => l.UserId == userId)
                            .OrderByDescending(l => l.ActionDate);
                }

                public IEnumerable<AuditLog> GetRecentLogs(int count)
                {
                        return _context.AuditLogs
                            .OrderByDescending(l => l.ActionDate)
                            .Take(count);
                }

                public IEnumerable<AuditLog> GetLogsByDateRange(DateTime start, DateTime end)
                {
                        return _context.AuditLogs
                            .Where(l => l.ActionDate >= start && l.ActionDate <= end)
                            .OrderByDescending(l => l.ActionDate);
                }
        }
}
