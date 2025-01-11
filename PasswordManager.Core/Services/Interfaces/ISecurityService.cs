using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Services.Interfaces
{
        public interface ISecurityService
        {
                string HashPassword(string password);
                bool VerifyPassword(string password, string hashedPassword);
                string GenerateStrongPassword(int length = 16);
                string GenerateTwoFactorKey();
                bool ValidateTwoFactorCode(string secretKey, string code);
        }
}
