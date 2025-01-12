using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Core.Services;
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
        public class AdminDashboardViewModel : ViewModelBase
        {
                private readonly IUserRepository _userRepository;
                private readonly IAuditLogRepository _auditLogRepository;
                private readonly IDialogService _dialogService;
                private readonly ISecurityService _securityService;

                public ObservableCollection<UserModel> Users { get; private set; }
                public ObservableCollection<AuditLogModel> RecentActivity { get; private set; }

                private UserModel _selectedUser;
                public UserModel SelectedUser
                {
                        get { return _selectedUser; }
                        set { SetProperty(ref _selectedUser, value); }
                }

                public ICommand DeactivateUserCommand { get; private set; }
                public ICommand ResetPasswordCommand { get; private set; }
                public ICommand RefreshCommand { get; private set; }

                public AdminDashboardViewModel(
                    IUserRepository userRepository,
                    IAuditLogRepository auditLogRepository,
                    IDialogService dialogService,
                    ISecurityService securityService)
                {
                        _userRepository = userRepository;
                        _auditLogRepository = auditLogRepository;
                        _dialogService = dialogService;
                        _securityService = securityService;

                        Users = new ObservableCollection<UserModel>();
                        RecentActivity = new ObservableCollection<AuditLogModel>();

                        DeactivateUserCommand = new RelayCommand(ExecuteDeactivateUser, CanExecuteUserAction);
                        ResetPasswordCommand = new RelayCommand(ExecuteResetPassword, CanExecuteUserAction);
                        RefreshCommand = new RelayCommand(obj => LoadData());

                        LoadData();
                }

                private void LoadData()
                {
                        var users = _userRepository.GetUsers()
                            .Select(u => new UserModel
                            {
                                    UserId = u.UserId,
                                    Username = u.Username,
                                    Email = u.Email,
                                    Role = u.Role.RoleName,
                                    IsActive = u.IsActive ?? false,
                                    LastLoginDate = u.LastLoginDate
                            }).ToList();

                        Users.Clear();
                        foreach (var user in users)
                        {
                                Users.Add(user);
                        }

                        var activities = _auditLogRepository.GetRecentLogs(20)
                            .Select(log => new AuditLogModel
                            {
                                    Username = log.User != null ? log.User.Username : "System",
                                    Action = log.Action,
                                    Details = log.Details,
                                    ActionDate = log.ActionDate ?? DateTime.Now
                            }).ToList();

                        RecentActivity.Clear();
                        foreach (var activity in activities)
                        {
                                RecentActivity.Add(activity);
                        }
                }

                private bool CanExecuteUserAction(object parameter)
                {
                        return SelectedUser != null &&
                               SelectedUser.UserId != SessionManager.CurrentUser.UserId;
                }

                private void ExecuteDeactivateUser(object parameter)
                {
                        if (_dialogService.ShowConfirmation(
                            string.Format("Are you sure you want to {0} user {1}?",
                                SelectedUser.IsActive ? "deactivate" : "activate",
                                SelectedUser.Username)))
                        {
                                try
                                {
                                        _userRepository.UpdateUserStatus(SelectedUser.UserId, !SelectedUser.IsActive);
                                        LoadData();
                                        _dialogService.ShowMessage("User status updated successfully.");
                                }
                                catch (Exception ex)
                                {
                                        _dialogService.ShowError("Failed to update user status: " + ex.Message);
                                }
                        }
                }

                private void ExecuteResetPassword(object parameter)
                {
                        if (_dialogService.ShowConfirmation(
                            string.Format("Are you sure you want to reset the password for user {0}?",
                                SelectedUser.Username)))
                        {
                                try
                                {
                                        string newPassword = _securityService.GenerateStrongPassword();
                                        _userRepository.ChangePassword(SelectedUser.UserId, newPassword);
                                        _dialogService.ShowMessage(
                                            string.Format("Password has been reset. New password: {0}\n\nPlease securely communicate this to the user.",
                                                newPassword));
                                }
                                catch (Exception ex)
                                {
                                        _dialogService.ShowError("Failed to reset password: " + ex.Message);
                                }
                        }
                }
        }
}
