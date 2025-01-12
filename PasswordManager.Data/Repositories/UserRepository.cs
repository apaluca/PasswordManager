using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories
{
        public class UserRepository : IUserRepository
        {
                private readonly PasswordManagerDataContext _context;
                private readonly ISecurityService _securityService;

                public UserRepository(PasswordManagerDataContext context, ISecurityService securityService)
                {
                        _context = context;
                        _securityService = securityService;
                }

                public User GetById(int userId)
                {
                        return _context.Users.FirstOrDefault(u => u.UserId == userId && u.IsActive.GetValueOrDefault());
                }

                public User GetByUsername(string username)
                {
                        return _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive.GetValueOrDefault());
                }

                public bool ValidateUser(string username, string password)
                {
                        var user = GetByUsername(username);
                        if (user == null) return false;

                        return _securityService.VerifyPassword(password, user.PasswordHash);
                }

                public void Create(string username, string password, string email, int roleId)
                {
                        var user = new User
                        {
                                Username = username,
                                PasswordHash = _securityService.HashPassword(password),
                                Email = email,
                                RoleId = roleId,
                                CreatedDate = DateTime.Now,
                                IsActive = true
                        };

                        _context.Users.InsertOnSubmit(user);
                        _context.SubmitChanges();
                }

                public void Update(User user)
                {
                        // Entity is already being tracked by context
                        _context.SubmitChanges();
                }

                public void ChangePassword(int userId, string newPassword)
                {
                        var user = GetById(userId);
                        if (user != null)
                        {
                                user.PasswordHash = _securityService.HashPassword(newPassword);
                                _context.SubmitChanges();
                        }
                }

                public void EnableTwoFactor(int userId, string secretKey)
                {
                        var user = GetById(userId);
                        if (user != null)
                        {
                                user.TwoFactorEnabled = true;
                                user.TwoFactorSecret = secretKey;
                                _context.SubmitChanges();
                        }
                }

                public void DisableTwoFactor(int userId)
                {
                        var user = GetById(userId);
                        if (user != null)
                        {
                                user.TwoFactorEnabled = false;
                                user.TwoFactorSecret = null;
                                _context.SubmitChanges();
                        }
                }

                public void SetLastLoginDate(int userId)
                {
                        var user = GetById(userId);
                        if (user != null)
                        {
                                user.LastLoginDate = DateTime.Now;
                                _context.SubmitChanges();
                        }
                }

                public bool IsUsernameTaken(string username)
                {
                        return _context.Users.Any(u => u.Username == username);
                }

                public bool IsEmailTaken(string email)
                {
                        return _context.Users.Any(u => u.Email == email);
                }

                public IEnumerable<User> GetUsers()
                {
                        return _context.Users.Where(u => u.IsActive.GetValueOrDefault());
                }

                public void UpdateUserStatus(int userId, bool isActive)
                {
                        var user = GetById(userId);
                        if (user != null)
                        {
                                user.IsActive = isActive;
                                _context.SubmitChanges();
                        }
                }
        }
}
