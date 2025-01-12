using PasswordManager.Core.Models;
using PasswordManager.Data.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Mappers
{
        public static class PasswordMapper
        {
                public static StoredPasswordModel ToModel(this StoredPassword entity)
                {
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
                                ExpirationDate = entity.CreatedDate?.AddMonths(3)
                        };
                }

                public static StoredPassword ToEntity(this StoredPasswordModel model)
                {
                        if (model == null) return null;

                        return new StoredPassword
                        {
                                PasswordId = model.Id,
                                UserId = model.UserId,
                                SiteName = model.SiteName,
                                SiteUrl = model.SiteUrl,
                                Username = model.Username,
                                EncryptedPassword = model.EncryptedPassword,
                                Notes = model.Notes,
                                CreatedDate = model.ExpirationDate?.AddMonths(-3),
                                ModifiedDate = model.ModifiedDate
                        };
                }
        }
}
