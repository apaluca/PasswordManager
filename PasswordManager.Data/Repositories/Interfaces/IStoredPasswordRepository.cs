using PasswordManager.Data.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories.Interfaces
{
        public interface IStoredPasswordRepository
        {
                StoredPassword GetById(int passwordId);
                IEnumerable<StoredPassword> GetByUserId(int userId);
                void Create(StoredPassword password);
                void Update(StoredPassword password);
                void Delete(int passwordId);
                IEnumerable<StoredPassword> Search(int userId, string searchTerm);
        }
}
