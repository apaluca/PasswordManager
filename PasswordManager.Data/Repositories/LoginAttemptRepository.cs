using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories
{
        public class LoginAttemptRepository : ILoginAttemptRepository
        {
                private readonly PasswordManagerDataContext _context;

                public LoginAttemptRepository(PasswordManagerDataContext context)
                {
                        _context = context;
                }

                public void RecordAttempt(string username, bool isSuccessful, string ipAddress, string userAgent)
                {
                        var attempt = new LoginAttempt
                        {
                                Username = username,
                                IsSuccessful = isSuccessful,
                                IPAddress = ipAddress,
                                UserAgent = userAgent,
                                AttemptDate = DateTime.Now
                        };

                        _context.LoginAttempts.InsertOnSubmit(attempt);
                        _context.SubmitChanges();
                }

                public int GetFailedAttempts(string username, TimeSpan window)
                {
                        var cutoffTime = DateTime.Now.Subtract(window);
                        return _context.LoginAttempts.Count(
                            la => la.Username == username &&
                                  !la.IsSuccessful &&
                                  la.AttemptDate >= cutoffTime);
                }

                public int GetFailedAttempts(int hours)
                {
                        var cutoffTime = DateTime.Now.AddHours(-hours);
                        return _context.LoginAttempts.Count(
                            la => !la.IsSuccessful &&
                            la.AttemptDate >= cutoffTime);
                }

                public IEnumerable<LoginAttempt> GetRecentAttempts(int count)
                {
                        return _context.LoginAttempts
                            .OrderByDescending(la => la.AttemptDate)
                            .Take(count);
                }

                public IEnumerable<LoginAttempt> GetAttemptsByDateRange(DateTime startDate, DateTime endDate)
                {
                        return _context.LoginAttempts.Where(
                            la => la.AttemptDate >= startDate &&
                            la.AttemptDate <= endDate);
                }
        }
}
