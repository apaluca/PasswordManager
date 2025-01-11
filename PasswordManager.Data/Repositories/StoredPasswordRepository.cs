using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories
{
        public class StoredPasswordRepository : IStoredPasswordRepository
        {
                private readonly PasswordManagerDataContext _context;
                private readonly IEncryptionService _encryptionService;

                public StoredPasswordRepository(PasswordManagerDataContext context, IEncryptionService encryptionService)
                {
                        _context = context;
                        _encryptionService = encryptionService;
                }

                public StoredPassword GetById(int passwordId)
                {
                        return _context.StoredPasswords.FirstOrDefault(p => p.PasswordId == passwordId);
                }

                public IEnumerable<StoredPassword> GetByUserId(int userId)
                {
                        return _context.StoredPasswords.Where(p => p.UserId == userId);
                }

                public void Create(StoredPassword password)
                {
                        password.EncryptedPassword = _encryptionService.Encrypt(password.EncryptedPassword);
                        password.CreatedDate = DateTime.Now;
                        password.ModifiedDate = DateTime.Now;

                        _context.StoredPasswords.InsertOnSubmit(password);
                        _context.SubmitChanges();
                }

                public void Update(StoredPassword password)
                {
                        password.ModifiedDate = DateTime.Now;
                        _context.SubmitChanges();
                }

                public void Delete(int passwordId)
                {
                        var password = GetById(passwordId);
                        if (password != null)
                        {
                                _context.StoredPasswords.DeleteOnSubmit(password);
                                _context.SubmitChanges();
                        }
                }

                public IEnumerable<StoredPassword> Search(int userId, string searchTerm)
                {
                        return _context.StoredPasswords
                            .Where(p => p.UserId == userId &&
                                (p.SiteName.Contains(searchTerm) ||
                                 p.Username.Contains(searchTerm) ||
                                 p.SiteUrl.Contains(searchTerm)));
                }
        }

}
