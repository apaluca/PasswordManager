using PasswordManager.Core.Models;
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
                StoredPasswordModel GetById(int passwordId);
                IEnumerable<StoredPasswordModel> GetByUserId(int userId);
                void Create(StoredPasswordModel password);
                void Update(StoredPasswordModel password);
                void Delete(int passwordId);
                IEnumerable<StoredPasswordModel> Search(int userId, string searchTerm);
        }
}
