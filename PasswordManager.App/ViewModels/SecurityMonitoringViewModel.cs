using PasswordManager.Core.Models;
using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Core.MVVM;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PasswordManager.Core.Services;
using PasswordManager.Data.Repositories;

namespace PasswordManager.App.ViewModels
{
        public class SecurityMonitoringViewModel : ViewModelBase
        {
                private readonly ILoginAttemptRepository _loginAttemptRepository;
                private readonly IDialogService _dialogService;
                private readonly IAuditLogRepository _auditLogRepository;

                public ObservableCollection<LoginAttemptModel> LoginAttempts { get; private set; }
                public ObservableCollection<IPAnalysisModel> IPAnalysis { get; private set; }

                private string _selectedTimeRange;
                public string SelectedTimeRange
                {
                        get => _selectedTimeRange;
                        set
                        {
                                if (SetProperty(ref _selectedTimeRange, value))
                                {
                                        LoadData();
                                }
                        }
                }

                private string _filterIPAddress;
                public string FilterIPAddress
                {
                        get => _filterIPAddress;
                        set
                        {
                                if (SetProperty(ref _filterIPAddress, value))
                                {
                                        LoadData();
                                }
                        }
                }

                public List<string> TimeRanges { get; } = new List<string>
                {
                    "Last Hour",
                    "Last 24 Hours",
                    "Last 7 Days",
                    "Last 30 Days"
                };

                private int _totalFailedAttempts;
                public int TotalFailedAttempts
                {
                        get => _totalFailedAttempts;
                        private set => SetProperty(ref _totalFailedAttempts, value);
                }

                private int _uniqueIPAddresses;
                public int UniqueIPAddresses
                {
                        get => _uniqueIPAddresses;
                        private set => SetProperty(ref _uniqueIPAddresses, value);
                }

                private double _averageAttemptsPerIP;
                public double AverageAttemptsPerIP
                {
                        get => _averageAttemptsPerIP;
                        private set => SetProperty(ref _averageAttemptsPerIP, value);
                }

                public ICommand RefreshCommand { get; }
                public ICommand ExportReportCommand { get; }
                public ICommand BlockIPCommand { get; }

                public SecurityMonitoringViewModel(
                    ILoginAttemptRepository loginAttemptRepository,
                    IDialogService dialogService,
                    IAuditLogRepository auditLogRepository)
                {
                        _loginAttemptRepository = loginAttemptRepository;
                        _dialogService = dialogService;
                        _auditLogRepository = auditLogRepository;

                        LoginAttempts = new ObservableCollection<LoginAttemptModel>();
                        IPAnalysis = new ObservableCollection<IPAnalysisModel>();
                        SelectedTimeRange = "Last 24 Hours";

                        RefreshCommand = new RelayCommand(_ => LoadData());
                        ExportReportCommand = new RelayCommand(ExecuteExportReport);
                        BlockIPCommand = new RelayCommand(ExecuteBlockIP, CanExecuteBlockIP);

                        LoadData();
                }

                private void LoadData()
                {
                        try
                        {
                                DateTime startDate = GetStartDateFromRange();
                                var attempts = _loginAttemptRepository.GetAttemptsByDateRange(startDate, DateTime.Now)
                                    .Where(a => string.IsNullOrWhiteSpace(FilterIPAddress) ||
                                               a.IPAddress.Contains(FilterIPAddress))
                                    .OrderByDescending(a => a.AttemptDate)
                                    .Select(a => new LoginAttemptModel
                                    {
                                            AttemptId = a.AttemptId,
                                            Username = a.Username,
                                            AttemptDate = a.AttemptDate,
                                            IsSuccessful = a.IsSuccessful,
                                            IPAddress = a.IPAddress,
                                            UserAgent = a.UserAgent
                                    });

                                LoginAttempts.Clear();
                                foreach (var attempt in attempts)
                                {
                                        LoginAttempts.Add(attempt);
                                }

                                // Calculate statistics
                                TotalFailedAttempts = attempts.Count(a => !a.IsSuccessful);
                                var ipGroups = attempts.GroupBy(a => a.IPAddress);
                                UniqueIPAddresses = ipGroups.Count();
                                AverageAttemptsPerIP = attempts.Count() / (double)UniqueIPAddresses;

                                // IP Analysis
                                var ipAnalysis = ipGroups.Select(g => new IPAnalysisModel
                                {
                                        IPAddress = g.Key,
                                        TotalAttempts = g.Count(),
                                        FailedAttempts = g.Count(a => !a.IsSuccessful),
                                        LastAttempt = g.Max(a => a.AttemptDate),
                                        SuccessRate = g.Count(a => a.IsSuccessful) / (double)g.Count(),
                                        RiskLevel = CalculateRiskLevel(g.Count(), g.Count(a => !a.IsSuccessful))
                                }).OrderByDescending(a => a.FailedAttempts);

                                IPAnalysis.Clear();
                                foreach (var analysis in ipAnalysis)
                                {
                                        IPAnalysis.Add(analysis);
                                }
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to load security data: " + ex.Message);
                        }
                }

                private DateTime GetStartDateFromRange()
                {
                        switch (SelectedTimeRange)
                        {
                                case "Last Hour":
                                        return DateTime.Now.AddHours(-1);
                                case "Last 7 Days":
                                        return DateTime.Now.AddDays(-7);
                                case "Last 30 Days":
                                        return DateTime.Now.AddDays(-30);
                                default:
                                        return DateTime.Now.AddDays(-1); // Default to "Last 24 Hours"
                        }
                }


                private string CalculateRiskLevel(int totalAttempts, int failedAttempts)
                {
                        double failureRate = failedAttempts / (double)totalAttempts;

                        if (failedAttempts >= 10 && failureRate > 0.8) return "High";
                        if (failedAttempts >= 5 && failureRate > 0.6) return "Medium";
                        return "Low";
                }

                private void ExecuteExportReport(object parameter)
                {
                        var report = new StringBuilder();
                        report.AppendLine("Security Analysis Report");
                        report.AppendLine($"Generated: {DateTime.Now}");
                        report.AppendLine($"Time Range: {SelectedTimeRange}");
                        report.AppendLine();
                        report.AppendLine("Summary Statistics:");
                        report.AppendLine($"Total Failed Attempts: {TotalFailedAttempts}");
                        report.AppendLine($"Unique IP Addresses: {UniqueIPAddresses}");
                        report.AppendLine($"Average Attempts per IP: {AverageAttemptsPerIP:F2}");
                        report.AppendLine();
                        report.AppendLine("High Risk IPs:");

                        foreach (var ip in IPAnalysis.Where(ip => ip.RiskLevel == "High"))
                        {
                                report.AppendLine($"IP: {ip.IPAddress}");
                                report.AppendLine($"  Failed Attempts: {ip.FailedAttempts}");
                                report.AppendLine($"  Success Rate: {ip.SuccessRate:P}");
                                report.AppendLine($"  Last Attempt: {ip.LastAttempt}");
                                report.AppendLine();
                        }

                        _dialogService.ShowMessage(report.ToString(), "Security Analysis Report");
                }

                private bool CanExecuteBlockIP(object parameter)
                {
                        return !string.IsNullOrWhiteSpace(FilterIPAddress);
                }

                private void ExecuteBlockIP(object parameter)
                {
                        if (_dialogService.ShowConfirmation(
                            $"Are you sure you want to block IP address {FilterIPAddress}?\n\n" +
                            "This will prevent all future login attempts from this IP address."))
                        {
                                // In a real implementation, this would add the IP to a blocklist
                                _dialogService.ShowMessage($"IP address {FilterIPAddress} has been blocked.");
                                _auditLogRepository.LogAction(
                                            SessionManager.CurrentUser.UserId,
                                            "Security_IPBlocked",
                                            $"Blocked IP address: {FilterIPAddress}",
                                            "localhost");
                        }
                }
        }
}
