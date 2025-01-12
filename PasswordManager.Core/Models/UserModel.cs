using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Models
{
        public class UserModel
        {
                public int UserId { get; set; }
                public string Username { get; set; }
                public string Email { get; set; }
                public string Role { get; set; }
                public bool IsActive { get; set; }
                public DateTime? LastLoginDate { get; set; }
                public bool TwoFactorEnabled { get; set; }
        }
}
