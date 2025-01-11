using PasswordManager.App.Views;
using PasswordManager.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordManager.App.Services
{
        public class NavigationService : INavigationService
        {
                public event EventHandler<NavigationEventArgs> Navigated;

                public void NavigateToMain()
                {
                        var mainWindow = new MainWindow();
                        var currentWindow = Application.Current.MainWindow;

                        Application.Current.MainWindow = mainWindow;
                        mainWindow.Show();

                        if (currentWindow != null)
                        {
                                currentWindow.Close();
                        }

                        OnNavigated(typeof(MainWindow));
                }

                public void NavigateToLogin()
                {
                        var loginWindow = new LoginWindow();
                        var currentWindow = Application.Current.MainWindow;

                        Application.Current.MainWindow = loginWindow;
                        loginWindow.Show();

                        if (currentWindow != null)
                        {
                                currentWindow.Close();
                        }

                        OnNavigated(typeof(LoginWindow));
                }

                protected virtual void OnNavigated(Type viewType)
                {
                        Navigated?.Invoke(this, new NavigationEventArgs { ViewType = viewType });
                }
        }
}
