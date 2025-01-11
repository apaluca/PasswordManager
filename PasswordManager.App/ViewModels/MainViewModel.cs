using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordManager.App.ViewModels
{
        public class MainViewModel : ViewModelBase
        {
                private readonly INavigationService _navigationService;
                private ViewModelBase _currentViewModel;

                public ViewModelBase CurrentViewModel
                {
                        get { return _currentViewModel; }
                        set { SetProperty(ref _currentViewModel, value); }
                }

                public string WelcomeMessage
                {
                        get { return "Welcome, " + SessionManager.CurrentUser.Username; }
                }

                public ICommand LogoutCommand { get; private set; }

                public MainViewModel(INavigationService navigationService, DashboardViewModel dashboardViewModel)
                {
                        _navigationService = navigationService;
                        CurrentViewModel = dashboardViewModel;
                        LogoutCommand = new RelayCommand(ExecuteLogout);
                }

                private void ExecuteLogout(object parameter)
                {
                        SessionManager.Logout();
                        _navigationService.NavigateToLogin();
                }
        }
}
