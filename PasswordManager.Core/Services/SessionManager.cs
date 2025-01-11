using PasswordManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Services
{
        public static class SessionManager
        {
                public static UserModel CurrentUser { get; set; }

                public static bool IsAuthenticated
                {
                        get { return CurrentUser != null; }
                }

                public static void Logout()
                {
                        CurrentUser = null;
                }
        }
}
