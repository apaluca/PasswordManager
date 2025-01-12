using PasswordManager.Data.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories.Interfaces
{
        public interface ILoginAttemptRepository
        {
                void RecordAttempt(string username, bool isSuccessful, string ipAddress, string userAgent);
                int GetFailedAttempts(string username, TimeSpan window);
                int GetFailedAttempts(int hours);
                IEnumerable<LoginAttempt> GetRecentAttempts(int count);
        }
}
