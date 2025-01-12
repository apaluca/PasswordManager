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

                public ICommand LogoutCommand { get; private set; }
                public ICommand NavigateCommand { get; private set; }

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
                        NavigateCommand = new RelayCommand(ExecuteNavigate);

                        NavigateToDefaultView();
                }

                private void NavigateToDefaultView()
                {
                        if (IsAdministrator)
                        {
                                CurrentViewModel = new AdminDashboardViewModel(
                                    _userRepository,
                                    _auditLogRepository,
                                    _dialogService,
                                    _securityService);
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
                        SessionManager.Logout();
                        _navigationService.NavigateToLogin();
                }

                private void ExecuteNavigate(object parameter)
                {
                        string viewName = parameter as string;
                        if (string.IsNullOrEmpty(viewName)) return;

                        switch (viewName)
                        {
                                case "Dashboard":
                                        NavigateToDefaultView();
                                        break;
                                case "PasswordManagement":
                                        CurrentViewModel = new PasswordManagementViewModel(
                                            _passwordRepository,
                                            _securityService,
                                            _encryptionService,
                                            _dialogService,
                                            _passwordStrengthService);
                                        break;
                        }
                }
        }
}
