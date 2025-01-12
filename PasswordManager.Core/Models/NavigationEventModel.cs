using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Models
{
        public class NavigationEventModel
        {
                public Type ViewType { get; set; }
                public object Parameter { get; set; }
        }
}
