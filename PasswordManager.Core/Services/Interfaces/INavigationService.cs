using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Core.Services.Interfaces
{
        public interface INavigationService
        {
                void NavigateToMain();
                void NavigateToLogin();
                event EventHandler<NavigationEventArgs> Navigated;
        }

        public class NavigationEventArgs : EventArgs
        {
                public Type ViewType { get; set; }
        }
}
