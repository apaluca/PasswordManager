using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordManager.App.ViewModels
{
        public class LoginViewModel : ViewModelBase
        {
                private readonly IUserRepository _userRepository;
                private readonly ISecurityService _securityService;
                private readonly ILoginAttemptRepository _loginAttemptRepository;
                private readonly IDialogService _dialogService;
                private readonly INavigationService _navigationService;

                public LoginViewModel(
                    IUserRepository userRepository,
                    ISecurityService securityService,
                    ILoginAttemptRepository loginAttemptRepository,
                    IDialogService dialogService,
                    INavigationService navigationService)
                {
                        _userRepository = userRepository;
                        _securityService = securityService;
                        _loginAttemptRepository = loginAttemptRepository;
                        _dialogService = dialogService;
                        _navigationService = navigationService;

                        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
                }

                private string _username;
                public string Username
                {
                        get { return _username; }
                        set
                        {
                                if (SetProperty(ref _username, value))
                                {
                                        ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                                }
                        }
                }

                private string _password;
                public string Password
                {
                        get { return _password; }
                        set
                        {
                                if (SetProperty(ref _password, value))
                                {
                                        ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                                }
                        }
                }

                public ICommand LoginCommand { get; private set; }

                public LoginViewModel(
                    IUserRepository userRepository,
                    ISecurityService securityService,
                    ILoginAttemptRepository loginAttemptRepository,
                    IDialogService dialogService)
                {
                        _userRepository = userRepository;
                        _securityService = securityService;
                        _loginAttemptRepository = loginAttemptRepository;
                        _dialogService = dialogService;

                        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
                }

                private bool CanExecuteLogin(object parameter)
                {
                        return !string.IsNullOrWhiteSpace(Username) &&
                               !string.IsNullOrWhiteSpace(Password) &&
                               !IsBusy;
                }

                private void ExecuteLogin(object parameter)
                {
                        try
                        {
                                IsBusy = true;
                                ClearError();

                                Task.Factory.StartNew(() =>
                                {
                                        var user = _userRepository.GetByUsername(Username);
                                        var isValid = user != null && _securityService.VerifyPassword(Password, user.PasswordHash);

                                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                        {
                                                if (!isValid)
                                                {
                                                        _loginAttemptRepository.RecordAttempt(Username, false, "localhost", "WPF Client");
                                                        SetError("Invalid username or password");
                                                        return;
                                                }

                                                _loginAttemptRepository.RecordAttempt(Username, true, "localhost", "WPF Client");

                                                // Store the user info in a session service or similar
                                                SessionManager.CurrentUser = user;

                                                // Navigate to main window
                                                _navigationService.NavigateToMain();
                                        });
                                })
                                .ContinueWith(t =>
                                {
                                        if (t.Exception != null)
                                        {
                                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                        SetError("An error occurred during login");
                                                        _dialogService.ShowError("Login failed. Please try again.");
                                                });
                                        }
                                }, TaskContinuationOptions.OnlyOnFaulted);
                        }
                        finally
                        {
                                IsBusy = false;
                        }
                }
        }
}
