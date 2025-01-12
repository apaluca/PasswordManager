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
                private ObservableCollection<StoredPasswordModel> _expiringPasswords;
                public ObservableCollection<StoredPasswordModel> ExpiringPasswords
                {
                        get { return _expiringPasswords; }
                        private set { SetProperty(ref _expiringPasswords, value); }
                }

                public ObservableCollection<StoredPasswordModel> RecentPasswords { get; private set; }
                public ObservableCollection<LoginAttemptModel> RecentLoginAttempts { get; private set; }
                public ObservableCollection<AuditLogModel> RecentAuditLogs { get; private set; }

                private int _totalPasswords;
                public int TotalPasswords
                {
                        get { return _totalPasswords; }
                        set { SetProperty(ref _totalPasswords, value); }
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

                        RefreshCommand = new RelayCommand(ExecuteRefresh);
                        AddPasswordCommand = new RelayCommand(ExecuteAddPassword);

                        LoadDashboardData();
                }

                private void LoadDashboardData()
                {
                        var userId = SessionManager.CurrentUser.UserId;

                        // Load passwords
                        var passwords = _passwordRepository.GetByUserId(userId)
                        .Select(p => new StoredPasswordModel
                        {
                                Id = p.PasswordId,
                                UserId = p.UserId,
                                SiteName = p.SiteName,
                                SiteUrl = p.SiteUrl,
                                Username = p.Username,
                                EncryptedPassword = p.EncryptedPassword,
                                Notes = p.Notes,
                                CreatedDate = p.CreatedDate,
                                ModifiedDate = p.ModifiedDate,
                                ExpirationDate = p.CreatedDate?.AddMonths(3)
                        }).ToList();
                        RecentPasswords.Clear();
                        foreach (var password in passwords.Take(5))
                        {
                                RecentPasswords.Add(password);
                        }
                        TotalPasswords = passwords.Count();

                        // Load additional data for admin/IT
                        if (IsAdministrator || IsITSpecialist)
                        {
                                var loginAttempts = _loginAttemptRepository.GetRecentAttempts(5)
                                    .Select(la => new LoginAttemptModel
                                    {
                                            AttemptId = la.AttemptId,
                                            Username = la.Username,
                                            AttemptDate = la.AttemptDate,
                                            IsSuccessful = la.IsSuccessful,
                                            IPAddress = la.IPAddress,
                                            UserAgent = la.UserAgent
                                    });

                                RecentLoginAttempts.Clear();
                                foreach (var attempt in loginAttempts)
                                {
                                        RecentLoginAttempts.Add(attempt);
                                }

                                var auditLogs = _auditLogRepository.GetRecentLogs(5)
                                    .Select(log => new AuditLogModel
                                    {
                                            LogId = log.LogId,
                                            UserId = log.UserId,
                                            ActionDate = log.ActionDate,
                                            Action = log.Action,
                                            Details = log.Details,
                                            IPAddress = log.IPAddress,
                                            Username = log.User?.Username ?? "System"
                                    });

                                RecentAuditLogs.Clear();
                                foreach (var log in auditLogs)
                                {
                                        RecentAuditLogs.Add(log);
                                }
                        }

                        // Check for expiring passwords
                        var thirtyDaysFromNow = DateTime.Now.AddDays(30);
                        var expiring = passwords
                            .Where(p => p.ExpirationDate != null && p.ExpirationDate <= thirtyDaysFromNow)
                            .OrderBy(p => p.ExpirationDate)
                            .ToList();

                        ExpiringPasswords.Clear();
                        foreach (var password in expiring)
                        {
                                ExpiringPasswords.Add(password);
                        }

                        if (expiring.Any())
                        {
                                _dialogService.ShowMessage($"You have {expiring.Count} passwords that will expire soon!");
                        }
                }

                private void ExecuteRefresh(object parameter)
                {
                        LoadDashboardData();
                }

                private void ExecuteAddPassword(object parameter)
                {
                        var viewModel = new PasswordEntryViewModel(
                            _passwordRepository,
                            _securityService,
                            _encryptionService,
                            _dialogService,
                            _passwordStrengthService);

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
