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
using PasswordManager.Core.Models;

namespace PasswordManager.App.ViewModels
{
        public class ITDashboardViewModel : ViewModelBase
        {
                private readonly ILoginAttemptRepository _loginAttemptRepository;
                private readonly IAuditLogRepository _auditLogRepository;
                private readonly IDialogService _dialogService;

                public ObservableCollection<SecurityIncidentModel> FailedLogins { get; private set; }
                public ObservableCollection<SecurityIncidentModel> SuspiciousActivity { get; private set; }

                private int _totalFailedAttempts;
                public int TotalFailedAttempts
                {
                        get { return _totalFailedAttempts; }
                        set { SetProperty(ref _totalFailedAttempts, value); }
                }

                private int _uniqueIPAddresses;
                public int UniqueIPAddresses
                {
                        get { return _uniqueIPAddresses; }
                        set { SetProperty(ref _uniqueIPAddresses, value); }
                }

                public ICommand RefreshCommand { get; private set; }
                public ICommand ExportReportCommand { get; private set; }

                public ITDashboardViewModel(
                    ILoginAttemptRepository loginAttemptRepository,
                    IAuditLogRepository auditLogRepository,
                    IDialogService dialogService)
                {
                        _loginAttemptRepository = loginAttemptRepository;
                        _auditLogRepository = auditLogRepository;
                        _dialogService = dialogService;

                        FailedLogins = new ObservableCollection<SecurityIncidentModel>();
                        SuspiciousActivity = new ObservableCollection<SecurityIncidentModel>();

                        RefreshCommand = new RelayCommand(obj => LoadData());
                        ExportReportCommand = new RelayCommand(ExecuteExportReport);

                        LoadData();
                }

                private void LoadData()
                {
                        var lastDay = DateTime.Now.AddHours(-24);
                        var failedAttempts = _loginAttemptRepository
                            .GetRecentAttempts(100)
                            .Where(x => !x.IsSuccessful && x.AttemptDate >= lastDay)
                            .ToList();

                        FailedLogins.Clear();
                        foreach (var attempt in failedAttempts)
                        {
                                FailedLogins.Add(new SecurityIncidentModel
                                {
                                        Timestamp = attempt.AttemptDate ?? DateTime.Now,
                                        Username = attempt.Username,
                                        IPAddress = attempt.IPAddress,
                                        Details = string.Format("Failed login attempt from {0}", attempt.UserAgent)
                                });
                        }

                        TotalFailedAttempts = failedAttempts.Count;
                        UniqueIPAddresses = failedAttempts.Select(f => f.IPAddress).Distinct().Count();

                        var suspiciousIPs = failedAttempts
                            .GroupBy(f => f.IPAddress)
                            .Where(g => g.Count() >= 3)
                            .Select(g => new SecurityIncidentModel
                            {
                                    Timestamp = g.Max(f => f.AttemptDate ?? DateTime.Now),
                                    IPAddress = g.Key,
                                    Details = string.Format("Multiple failed attempts ({0})", g.Count()),
                                    Severity = "High"
                            })
                            .ToList();

                        SuspiciousActivity.Clear();
                        foreach (var incident in suspiciousIPs)
                        {
                                SuspiciousActivity.Add(incident);
                        }
                }

                private void ExecuteExportReport(object parameter)
                {
                        try
                        {
                                var report = new StringBuilder();
                                report.AppendLine("Security Report");
                                report.AppendLine(string.Format("Generated: {0}", DateTime.Now));
                                report.AppendLine(string.Format("Total Failed Attempts (24h): {0}", TotalFailedAttempts));
                                report.AppendLine(string.Format("Unique IP Addresses: {0}", UniqueIPAddresses));
                                report.AppendLine("\nSuspicious Activity:");

                                foreach (var incident in SuspiciousActivity)
                                {
                                        report.AppendLine(string.Format("{0}: {1} from {2}",
                                            incident.Timestamp, incident.Details, incident.IPAddress));
                                }

                                _dialogService.ShowMessage(report.ToString(), "Security Report");
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to generate report: " + ex.Message);
                        }
                }
        }
}
