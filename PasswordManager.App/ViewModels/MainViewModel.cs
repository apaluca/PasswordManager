using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Data.Repositories.Interfaces;
using System.Windows;
using PasswordManager.App.Views;

namespace PasswordManager.App.ViewModels
{
        public class MainViewModel : ViewModelBase
        {
                private readonly INavigationService _navigationService;
                private readonly IUserRepository _userRepository;
                private readonly IStoredPasswordRepository _passwordRepository;
                private readonly IAuditLogRepository _auditLogRepository;
                private readonly ILoginAttemptRepository _loginAttemptRepository;
                private readonly IDialogService _dialogService;
                private readonly ISecurityService _securityService;
                private readonly IEncryptionService _encryptionService;
                private readonly IPasswordStrengthService _passwordStrengthService;
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

                public bool IsAdministrator
                {
                        get { return SessionManager.CurrentUser.Role == "Administrator"; }
                }

                public bool IsITSpecialist
                {
                        get { return SessionManager.CurrentUser.Role == "IT_Specialist"; }
                }

                public bool IsTwoFactorEnabled
                {
                        get { return SessionManager.CurrentUser?.TwoFactorEnabled ?? false; }
                }

                public ICommand LogoutCommand { get; private set; }
                public ICommand ChangePasswordCommand { get; private set; }
                public ICommand NavigateCommand { get; private set; }
                public ICommand SetupTwoFactorCommand { get; private set; }
                public ICommand ToggleTwoFactorCommand { get; private set; }

                public MainViewModel(
                        INavigationService navigationService,
                        IUserRepository userRepository,
                        IStoredPasswordRepository passwordRepository,
                        IAuditLogRepository auditLogRepository,
                        ILoginAttemptRepository loginAttemptRepository,
                        IDialogService dialogService,
                        ISecurityService securityService,
                        IEncryptionService encryptionService,
                        IPasswordStrengthService passwordStrengthService)
                {
                        _navigationService = navigationService;
                        _userRepository = userRepository;
                        _passwordRepository = passwordRepository;
                        _auditLogRepository = auditLogRepository;
                        _loginAttemptRepository = loginAttemptRepository;
                        _dialogService = dialogService;
                        _securityService = securityService;
                        _encryptionService = encryptionService;
                        _passwordStrengthService = passwordStrengthService;

                        LogoutCommand = new RelayCommand(ExecuteLogout);
                        ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);
                        NavigateCommand = new RelayCommand(ExecuteNavigate);
                        SetupTwoFactorCommand = new RelayCommand(_ => ExecuteSetupTwoFactor());
                        ToggleTwoFactorCommand = new RelayCommand(_ => ExecuteToggleTwoFactor());

                        NavigateToDefaultView();
                }

                private void NavigateToDefaultView()
                {
                        if (IsAdministrator)
                        {
                                CurrentViewModel = new DashboardViewModel(
                                    _passwordRepository,
                                    _loginAttemptRepository,
                                    _auditLogRepository,
                                    _dialogService,
                                    _securityService,
                                    _encryptionService,
                                    _passwordStrengthService);
                        }
                        else if (IsITSpecialist)
                        {
                                CurrentViewModel = new ITDashboardViewModel(
                                    _loginAttemptRepository,
                                    _auditLogRepository,
                                    _dialogService);
                        }
                        else
                        {
                                CurrentViewModel = new DashboardViewModel(
                                    _passwordRepository,
                                    _loginAttemptRepository,
                                    _auditLogRepository,
                                    _dialogService,
                                    _securityService,
                                    _encryptionService,
                                    _passwordStrengthService);
                        }
                }

                private void ExecuteLogout(object parameter)
                {
                        try
                        {
                                _auditLogRepository.LogAction(
                                    SessionManager.CurrentUser.UserId,
                                    "Security_Logout",
                                    $"User {SessionManager.CurrentUser.Username} logged out",
                                    "localhost");
                                
                                CurrentViewModel = null;
                                SessionManager.Logout();
                                _navigationService.NavigateToLogin();
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Error during logout: " + ex.Message);
                        }
                }

                private void ExecuteChangePassword(object parameter)
                {
                        var viewModel = new ChangePasswordViewModel(
                            _userRepository,
                            _securityService,
                            _passwordStrengthService,
                            _dialogService,
                            _auditLogRepository);

                        var window = new ChangePasswordWindow
                        {
                                Owner = Application.Current.MainWindow,
                                DataContext = viewModel
                        };

                        viewModel.RequestClose += (s, e) =>
                        {
                                window.DialogResult = true;
                                window.Close();
                        };

                        window.ShowDialog();
                }

                private void ExecuteNavigate(object parameter)
                {
                        string viewName = parameter as string;
                        if (string.IsNullOrEmpty(viewName)) return;

                        switch (viewName)
                        {
                                case "Dashboard":
                                        if (IsAdministrator)
                                        {
                                                CurrentViewModel = new DashboardViewModel(
                                                    _passwordRepository,
                                                    _loginAttemptRepository,
                                                    _auditLogRepository,
                                                    _dialogService,
                                                    _securityService,
                                                    _encryptionService,
                                                    _passwordStrengthService);
                                        }
                                        else if (IsITSpecialist)
                                        {
                                                CurrentViewModel = new ITDashboardViewModel(
                                                    _loginAttemptRepository,
                                                    _auditLogRepository,
                                                    _dialogService);
                                        }
                                        else
                                        {
                                                CurrentViewModel = new DashboardViewModel(
                                                    _passwordRepository,
                                                    _loginAttemptRepository,
                                                    _auditLogRepository,
                                                    _dialogService,
                                                    _securityService,
                                                    _encryptionService,
                                                    _passwordStrengthService);
                                        }
                                        break;

                                case "PasswordManagement":
                                        CurrentViewModel = new PasswordManagementViewModel(
                                            _passwordRepository,
                                            _securityService,
                                            _encryptionService,
                                            _dialogService,
                                            _passwordStrengthService,
                                            _auditLogRepository);
                                        break;

                                case "UserManagement":
                                        if (IsAdministrator)
                                        {
                                                CurrentViewModel = new AdminDashboardViewModel(
                                                    _userRepository,
                                                    _auditLogRepository,
                                                    _dialogService,
                                                    _securityService);
                                        }
                                        break;

                                case "SecurityMonitoring":
                                        if (IsITSpecialist)
                                        {
                                                CurrentViewModel = new SecurityMonitoringViewModel(
                                                    _loginAttemptRepository,
                                                    _dialogService,
                                                    _auditLogRepository);
                                        }
                                        break;
                        }
                }

                private void ExecuteSetupTwoFactor()
                {
                        var viewModel = new TwoFactorSetupViewModel(
                            _securityService,
                            _userRepository,
                            _dialogService,
                            _auditLogRepository);

                        var window = new TwoFactorSetupWindow
                        {
                                Owner = Application.Current.MainWindow,
                                DataContext = viewModel
                        };

                        viewModel.SetupCompleted += (s, e) =>
                        {
                                SessionManager.CurrentUser.TwoFactorEnabled = true;
                                OnPropertyChanged(nameof(IsTwoFactorEnabled));
                                window.Close();
                        };

                        window.ShowDialog();
                }

                private void ExecuteToggleTwoFactor()
                {
                        if (IsTwoFactorEnabled)
                        {
                                if (_dialogService.ShowConfirmation(
                                    "Are you sure you want to disable two-factor authentication?\n\n" +
                                    "This will make your account less secure."))
                                {
                                        try
                                        {
                                                _userRepository.DisableTwoFactor(SessionManager.CurrentUser.UserId);
                                                _auditLogRepository.LogAction(
                                                    SessionManager.CurrentUser.UserId,
                                                    "Security_2FADisabled",
                                                    "Two-factor authentication disabled",
                                                    "localhost");
                                                SessionManager.CurrentUser.TwoFactorEnabled = false;
                                                OnPropertyChanged(nameof(IsTwoFactorEnabled));
                                                _dialogService.ShowMessage("Two-factor authentication has been disabled.");
                                        }
                                        catch (Exception ex)
                                        {
                                                _dialogService.ShowError("Failed to disable two-factor authentication: " + ex.Message);
                                        }
                                }
                        }
                        else
                        {
                                var viewModel = new TwoFactorSetupViewModel(
                                    _securityService,
                                    _userRepository,
                                    _dialogService,
                                    _auditLogRepository);

                                var window = new TwoFactorSetupWindow
                                {
                                        Owner = Application.Current.MainWindow,
                                        DataContext = viewModel
                                };

                                viewModel.SetupCompleted += (s, e) =>
                                {
                                        SessionManager.CurrentUser.TwoFactorEnabled = true;
                                        OnPropertyChanged(nameof(IsTwoFactorEnabled));
                                        window.Close();
                                };

                                window.ShowDialog();
                        }
                }
        }
}
