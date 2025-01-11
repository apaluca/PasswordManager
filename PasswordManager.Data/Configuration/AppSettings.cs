using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Configuration
{
        public class AppSettings
        {
                public string ConnectionString { get; set; }
                public string EncryptionKey { get; set; }
        }
}
