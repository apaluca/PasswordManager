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
using PasswordManager.Core.Services;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories;

namespace PasswordManager.App.ViewModels
{
        public class LoginViewModel : ViewModelBase
        {
                private readonly IUserRepository _userRepository;
                private readonly ISecurityService _securityService;
                private readonly ILoginAttemptRepository _loginAttemptRepository;
                private readonly IDialogService _dialogService;
                private readonly INavigationService _navigationService;
                private readonly IAuditLogRepository _auditLogRepository;

                private string _username;
                private string _password;
                private string _twoFactorCode;
                private bool _isTwoFactorRequired;
                private User _currentUser;

                public string Username
                {
                        get => _username;
                        set
                        {
                                SetProperty(ref _username, value);
                                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                        }
                }

                public string Password
                {
                        get => _password;
                        set
                        {
                                SetProperty(ref _password, value);
                                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                        }
                }

                public string TwoFactorCode
                {
                        get => _twoFactorCode;
                        set
                        {
                                SetProperty(ref _twoFactorCode, value);
                                ((RelayCommand)VerifyTwoFactorCommand).RaiseCanExecuteChanged();
                        }
                }

                public bool IsTwoFactorRequired
                {
                        get => _isTwoFactorRequired;
                        private set => SetProperty(ref _isTwoFactorRequired, value);
                }

                public ICommand LoginCommand { get; }
                public ICommand VerifyTwoFactorCommand { get; }

                public LoginViewModel(
                    IUserRepository userRepository,
                    ISecurityService securityService,
                    ILoginAttemptRepository loginAttemptRepository,
                    IDialogService dialogService,
                    INavigationService navigationService,
                    IAuditLogRepository auditLogRepository)
                {
                        _userRepository = userRepository;
                        _securityService = securityService;
                        _loginAttemptRepository = loginAttemptRepository;
                        _dialogService = dialogService;
                        _navigationService = navigationService;
                        _auditLogRepository = auditLogRepository;

                        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
                        VerifyTwoFactorCommand = new RelayCommand(ExecuteVerifyTwoFactor, CanExecuteVerifyTwoFactor);
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

                                _currentUser = _userRepository.GetByUsername(Username);
                                bool isValid = _currentUser != null &&
                                              _securityService.VerifyPassword(Password, _currentUser.PasswordHash);

                                if (!isValid)
                                {
                                        _loginAttemptRepository.RecordAttempt(Username, false, "localhost", "WPF Client");
                                        _auditLogRepository.LogAction(
                                            null,
                                            "Security_FailedLogin",
                                            $"Failed login attempt for username: {Username}",
                                            "localhost");
                                        SetError("Invalid username or password");
                                        IsBusy = false;
                                        return;
                                }

                                if (_currentUser.TwoFactorEnabled ?? false)
                                {
                                        IsTwoFactorRequired = true;
                                        _auditLogRepository.LogAction(
                                            _currentUser.UserId,
                                            "Security_2FARequired",
                                            $"2FA verification required for user {Username}",
                                            "localhost");
                                        IsBusy = false;
                                        return;
                                }

                                CompleteLogin();
                        }
                        catch (Exception ex)
                        {
                                SetError("An error occurred during login: " + ex.Message);
                                IsBusy = false;
                        }
                }

                private void CompleteLogin()
                {
                        try
                        {
                                _loginAttemptRepository.RecordAttempt(
                                            Username,
                                            true,
                                            "localhost",
                                            "WPF Client");

                                _auditLogRepository.LogAction(
                                            _currentUser.UserId,
                                            "Security_SuccessfulLogin",
                                            $"Successful login for user {Username}",
                                            "localhost");

                                _userRepository.SetLastLoginDate(_currentUser.UserId);

                                SessionManager.CurrentUser = new Core.Models.UserModel
                                {
                                        UserId = _currentUser.UserId,
                                        Username = _currentUser.Username,
                                        Email = _currentUser.Email,
                                        Role = _currentUser.Role.RoleName,
                                        IsActive = _currentUser.IsActive ?? false,
                                        LastLoginDate = _currentUser.LastLoginDate,
                                        TwoFactorEnabled = _currentUser.TwoFactorEnabled ?? false
                                };

                                _navigationService.NavigateToMain();
                        }
                        finally
                        {
                                IsBusy = false;
                        }
                }

                private bool CanExecuteVerifyTwoFactor(object parameter)
                {
                        return IsTwoFactorRequired &&
                               !string.IsNullOrWhiteSpace(TwoFactorCode) &&
                               TwoFactorCode.Length == 6 &&
                               !IsBusy;
                }

                private void ExecuteVerifyTwoFactor(object parameter)
                {
                        try
                        {
                                IsBusy = true;
                                ClearError();

                                if (_securityService.ValidateTwoFactorCode(_currentUser.TwoFactorSecret, TwoFactorCode))
                                {
                                        CompleteLogin();
                                }
                                else
                                {
                                        _loginAttemptRepository.RecordAttempt(
                                            Username,
                                            false,
                                            "localhost",
                                            "WPF Client - 2FA Failed");
                                        SetError("Invalid verification code");
                                }
                        }
                        catch (Exception ex)
                        {
                                SetError("An error occurred during verification: " + ex.Message);
                        }
                        finally
                        {
                                IsBusy = false;
                        }
                }
        }
}
