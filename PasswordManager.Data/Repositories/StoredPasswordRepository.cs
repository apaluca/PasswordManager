using PasswordManager.Core.Models;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Mappers;
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

                public StoredPasswordModel GetById(int passwordId)
                {
                        var entity = _context.StoredPasswords.FirstOrDefault(p => p.PasswordId == passwordId);
                        return entity?.ToModel();
                }

                public IEnumerable<StoredPasswordModel> GetByUserId(int userId)
                {
                        return _context.StoredPasswords
                            .Where(p => p.UserId == userId)
                            .Select(p => p.ToModel());
                }

                public void Create(StoredPasswordModel password)
                {
                        var entity = password.ToEntity();
                        _context.StoredPasswords.InsertOnSubmit(entity);
                        _context.SubmitChanges();
                        password.Id = entity.PasswordId; // Update the ID after insert
                }

                public void Update(StoredPasswordModel password)
                {
                        var entity = _context.StoredPasswords.FirstOrDefault(p => p.PasswordId == password.Id);
                        if (entity != null)
                        {
                                entity.SiteName = password.SiteName;
                                entity.SiteUrl = password.SiteUrl;
                                entity.Username = password.Username;
                                entity.EncryptedPassword = password.EncryptedPassword;
                                entity.Notes = password.Notes;
                                entity.CreatedDate = password.ExpirationDate?.AddMonths(-3);
                                entity.ModifiedDate = DateTime.Now;

                                _context.SubmitChanges();
                        }
                }

                public void Delete(int passwordId)
                {
                        var password = _context.StoredPasswords.FirstOrDefault(p => p.PasswordId == passwordId);
                        if (password != null)
                        {
                                _context.StoredPasswords.DeleteOnSubmit(password);
                                _context.SubmitChanges();
                        }
                }

                public IEnumerable<StoredPasswordModel> Search(int userId, string searchTerm)
                {
                        return _context.StoredPasswords
                            .Where(p => p.UserId == userId &&
                                (p.SiteName.Contains(searchTerm) ||
                                 p.Username.Contains(searchTerm) ||
                                 p.SiteUrl.Contains(searchTerm)))
                            .Select(p => p.ToModel());
                }
        }
}
