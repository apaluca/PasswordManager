using PasswordManager.Core.Models;
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

                public StoredPasswordModel GetById(int passwordId)
                {
                        var entity = _context.StoredPasswords.FirstOrDefault(p => p.PasswordId == passwordId);
                        if (entity == null) return null;

                        return new StoredPasswordModel
                        {
                                Id = entity.PasswordId,
                                UserId = entity.UserId,
                                SiteName = entity.SiteName,
                                SiteUrl = entity.SiteUrl,
                                Username = entity.Username,
                                EncryptedPassword = entity.EncryptedPassword,
                                Notes = entity.Notes,
                                CreatedDate = entity.CreatedDate,
                                ModifiedDate = entity.ModifiedDate,
                                ExpirationDate = entity.ExpirationDate
                        };
                }

                public IEnumerable<StoredPasswordModel> GetByUserId(int userId)
                {
                        return _context.StoredPasswords
                            .Where(p => p.UserId == userId)
                            .Select(p => new StoredPasswordModel
                            {
                                    Id = p.PasswordId,
                                    UserId = p.UserId,
                                    SiteName = p.SiteName,
                                    SiteUrl = p.SiteUrl,
                                    Username = p.Username,
                                    EncryptedPassword = p.EncryptedPassword,
                                    Notes = p.Notes,
                                    CreatedDate = p.CreatedDate,
                                    ModifiedDate = p.ModifiedDate,
                                    ExpirationDate = p.ExpirationDate
                            });
                }

                public void Create(StoredPasswordModel password)
                {
                        var entity = new StoredPassword
                        {
                                UserId = password.UserId,
                                SiteName = password.SiteName,
                                SiteUrl = password.SiteUrl,
                                Username = password.Username,
                                EncryptedPassword = password.EncryptedPassword,
                                Notes = password.Notes,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                ExpirationDate = password.ExpirationDate
                        };

                        _context.StoredPasswords.InsertOnSubmit(entity);
                        _context.SubmitChanges();
                        password.Id = entity.PasswordId;
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
                                entity.ModifiedDate = DateTime.Now;
                                entity.ExpirationDate = password.ExpirationDate;

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
                            .Select(p => new StoredPasswordModel
                            {
                                    Id = p.PasswordId,
                                    UserId = p.UserId,
                                    SiteName = p.SiteName,
                                    SiteUrl = p.SiteUrl,
                                    Username = p.Username,
                                    EncryptedPassword = p.EncryptedPassword,
                                    Notes = p.Notes,
                                    CreatedDate = p.CreatedDate,
                                    ModifiedDate = p.ModifiedDate,
                                    ExpirationDate = p.ExpirationDate
                            });
                }
        }
}
