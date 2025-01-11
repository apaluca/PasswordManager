using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Services.Interfaces
{
        public interface IPasswordStrengthService
        {
                PasswordStrength CheckStrength(string password);
                string GetStrengthDescription(PasswordStrength strength);
                string GetStrengthColor(PasswordStrength strength);
        }
}
