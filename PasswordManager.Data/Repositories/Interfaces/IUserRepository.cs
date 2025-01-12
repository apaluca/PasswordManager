using PasswordManager.Data.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Repositories.Interfaces
{
        public interface IUserRepository
        {
                User GetById(int userId);
                User GetByUsername(string username);
                bool ValidateUser(string username, string password);
                void Create(string username, string password, string email, int roleId);
                void Update(User user);
                void ChangePassword(int userId, string newPassword);
                void EnableTwoFactor(int userId, string secretKey);
                void DisableTwoFactor(int userId);
                void SetLastLoginDate(int userId);
                bool IsUsernameTaken(string username);
                bool IsEmailTaken(string email);
                IEnumerable<User> GetUsers();
                void UpdateUserStatus(int userId, bool isActive);
        }
}
