using Microsoft.Extensions.DependencyInjection;
using PasswordManager.App.ViewModels;
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
                private readonly IServiceProvider _serviceProvider;

                public NavigationService(IServiceProvider serviceProvider)
                {
                        _serviceProvider = serviceProvider;
                }

                public void NavigateToMain()
                {
                        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
                        var mainWindow = new MainWindow
                        {
                                DataContext = mainViewModel
                        };
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
                        var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                        var loginWindow = new LoginWindow(loginViewModel);
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
