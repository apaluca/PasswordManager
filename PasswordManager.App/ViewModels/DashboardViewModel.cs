using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PasswordManager.App.Views;
using PasswordManager.Core.Services.Interfaces;
using System.Windows;
using PasswordManager.Core.Models;

namespace PasswordManager.App.ViewModels
{
        public class DashboardViewModel : ViewModelBase
        {
                private readonly IStoredPasswordRepository _passwordRepository;
                private readonly ILoginAttemptRepository _loginAttemptRepository;
                private readonly IAuditLogRepository _auditLogRepository;
                private readonly IDialogService _dialogService;
                private readonly ISecurityService _securityService;
                private readonly IEncryptionService _encryptionService;
                private readonly IPasswordStrengthService _passwordStrengthService;
                public ObservableCollection<StoredPasswordModel> ExpiringPasswords { get; private set; }
                public ObservableCollection<StoredPasswordModel> RecentPasswords { get; private set; }
                public ObservableCollection<SecurityMetric> SecurityMetrics { get; private set; }
                public ObservableCollection<LoginAttemptModel> RecentLoginAttempts { get; private set; }
                public ObservableCollection<AuditLogModel> RecentAuditLogs { get; private set; }

                private int _totalPasswords;
                public int TotalPasswords
                {
                        get => _totalPasswords;
                        set => SetProperty(ref _totalPasswords, value);
                }

                private int _expiringPasswordCount;
                public int ExpiringPasswordCount
                {
                        get => _expiringPasswordCount;
                        set => SetProperty(ref _expiringPasswordCount, value);
                }

                private int _weakPasswordCount;
                public int WeakPasswordCount
                {
                        get => _weakPasswordCount;
                        set => SetProperty(ref _weakPasswordCount, value);
                }

                private int _failedLoginAttempts;
                public int FailedLoginAttempts
                {
                        get { return _failedLoginAttempts; }
                        set { SetProperty(ref _failedLoginAttempts, value); }
                }

                public bool IsAdministrator
                {
                        get { return SessionManager.CurrentUser.Role == "Administrator"; }
                }

                public bool IsITSpecialist
                {
                        get { return SessionManager.CurrentUser.Role == "IT_Specialist"; }
                }

                public ICommand RefreshCommand { get; private set; }
                public ICommand AddPasswordCommand { get; private set; }

                public DashboardViewModel(
                        IStoredPasswordRepository passwordRepository,
                        ILoginAttemptRepository loginAttemptRepository,
                        IAuditLogRepository auditLogRepository,
                        IDialogService dialogService,
                        ISecurityService securityService,
                        IEncryptionService encryptionService,
                        IPasswordStrengthService passwordStrengthService)
                {
                        _passwordRepository = passwordRepository;
                        _loginAttemptRepository = loginAttemptRepository;
                        _auditLogRepository = auditLogRepository;
                        _dialogService = dialogService;
                        _securityService = securityService;
                        _encryptionService = encryptionService;
                        _passwordStrengthService = passwordStrengthService;

                        RecentPasswords = new ObservableCollection<StoredPasswordModel>();
                        RecentLoginAttempts = new ObservableCollection<LoginAttemptModel>();
                        RecentAuditLogs = new ObservableCollection<AuditLogModel>();
                        ExpiringPasswords = new ObservableCollection<StoredPasswordModel>();
                        SecurityMetrics = new ObservableCollection<SecurityMetric>();

                        RefreshCommand = new RelayCommand(_ => LoadDashboardData());
                        AddPasswordCommand = new RelayCommand(ExecuteAddPassword);

                        LoadDashboardData();
                }

                private void LoadDashboardData()
                {
                        var userId = SessionManager.CurrentUser.UserId;
                        var passwords = _passwordRepository.GetByUserId(userId).ToList();

                        // Update basic metrics
                        TotalPasswords = passwords.Count;

                        // Get recent passwords
                        var recentPasswords = passwords
                            .OrderByDescending(p => p.ModifiedDate)
                            .Take(5);

                        RecentPasswords.Clear();
                        foreach (var password in recentPasswords)
                        {
                                RecentPasswords.Add(password);
                        }

                        // Get expiring passwords (within 30 days)
                        var thirtyDaysFromNow = DateTime.Now.AddDays(30);
                        var expiring = passwords
                            .Where(p => p.ExpirationDate <= thirtyDaysFromNow)
                            .OrderBy(p => p.ExpirationDate);

                        ExpiringPasswords.Clear();
                        foreach (var password in expiring)
                        {
                                ExpiringPasswords.Add(password);
                        }
                        ExpiringPasswordCount = expiring.Count();

                        // Calculate security metrics
                        SecurityMetrics.Clear();
                        SecurityMetrics.Add(new SecurityMetric
                        {
                                Name = "Total Passwords",
                                Value = TotalPasswords.ToString(),
                                Status = "Info"
                        });
                        SecurityMetrics.Add(new SecurityMetric
                        {
                                Name = "Expiring Soon",
                                Value = ExpiringPasswordCount.ToString(),
                                Status = ExpiringPasswordCount > 0 ? "Warning" : "Good"
                        });
                        SecurityMetrics.Add(new SecurityMetric
                        {
                                Name = "2FA Status",
                                Value = SessionManager.CurrentUser.TwoFactorEnabled ? "Enabled" : "Disabled",
                                Status = SessionManager.CurrentUser.TwoFactorEnabled ? "Good" : "Warning"
                        });
                }

                private void ExecuteAddPassword(object parameter)
                {
                        var viewModel = new PasswordEntryViewModel(
                            _passwordRepository,
                            _securityService,
                            _encryptionService,
                            _dialogService,
                            _passwordStrengthService,
                            _auditLogRepository);

                        var window = new PasswordEntryWindow
                        {
                                DataContext = viewModel,
                                Owner = Application.Current.MainWindow
                        };

                        viewModel.RequestClose += (s, e) =>
                        {
                                window.DialogResult = true;
                                window.Close();
                                LoadDashboardData();
                        };

                        window.ShowDialog();
                }

                private void ExecuteEditPassword(StoredPasswordModel password)
                {
                        var viewModel = new PasswordEntryViewModel(
                            _passwordRepository,
                            _securityService,
                            _encryptionService,
                            _dialogService,
                            _passwordStrengthService,
                            _auditLogRepository,
                            password);

                        var window = new PasswordEntryWindow
                        {
                                DataContext = viewModel,
                                Owner = Application.Current.MainWindow
                        };

                        viewModel.RequestClose += (s, e) =>
                        {
                                window.DialogResult = true;
                                window.Close();
                                LoadDashboardData();
                        };

                        window.ShowDialog();
                }
        }
}
