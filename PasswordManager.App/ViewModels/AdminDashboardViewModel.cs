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
using PasswordManager.Data.DataContext;
using System.Runtime.Remoting.Contexts;
using System.Windows.Controls;
using System.Windows;

namespace PasswordManager.App.ViewModels
{
        public class AdminDashboardViewModel : ViewModelBase
        {
                private readonly IUserRepository _userRepository;
                private readonly IAuditLogRepository _auditLogRepository;
                private readonly IDialogService _dialogService;
                private readonly ISecurityService _securityService;
                private IEnumerable<Role> _availableRoles;
                public IEnumerable<Role> AvailableRoles
                {
                        get
                        {
                                if (_availableRoles == null)
                                {
                                        _availableRoles = _userRepository.GetAllRoles();
                                }
                                return _availableRoles;
                        }
                }

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
                public ICommand AddUserCommand { get; private set; }

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

                        AddUserCommand = new RelayCommand(_ => ExecuteAddUser(null));
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

                        var activities = _auditLogRepository.GetSystemActivityLogs(20)
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
                                        _auditLogRepository.LogAction(
                                            SessionManager.CurrentUser.UserId,
                                            "User_StatusChange",
                                            $"User {SelectedUser.Username} {(SelectedUser.IsActive ? "deactivated" : "activated")}",
                                            "localhost");
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
                                        _auditLogRepository.LogAction(
                                            SessionManager.CurrentUser.UserId,
                                            "User_PasswordReset",
                                            $"Password reset for user {SelectedUser.Username}",
                                            "localhost");
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

                private void ExecuteAddUser(object parameter)
                {
                        try
                        {
                                string username = _dialogService.ShowPrompt("Enter username:");
                                if (string.IsNullOrWhiteSpace(username)) return;

                                string email = _dialogService.ShowPrompt("Enter email:");
                                if (string.IsNullOrWhiteSpace(email)) return;

                                // Show role selection dialog
                                var roleDialog = new Window
                                {
                                        Title = "Select Role",
                                        Width = 300,
                                        Height = 200,
                                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                        Owner = Application.Current.MainWindow
                                };

                                var panel = new StackPanel { Margin = new Thickness(10) };
                                var label = new TextBlock { Text = "Select user role:", Margin = new Thickness(0, 0, 0, 5) };
                                var comboBox = new ComboBox
                                {
                                        Margin = new Thickness(0, 0, 0, 10),
                                        ItemsSource = AvailableRoles,
                                        DisplayMemberPath = "RoleName",
                                        SelectedValuePath = "RoleId"
                                };
                                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                                var okButton = new Button { Content = "OK", Width = 75, Height = 25, Margin = new Thickness(0, 0, 5, 0), IsEnabled = false };
                                var cancelButton = new Button { Content = "Cancel", Width = 75, Height = 25 };

                                // Enable OK button only when a role is selected
                                comboBox.SelectionChanged += (s, e) => okButton.IsEnabled = comboBox.SelectedItem != null;

                                buttonPanel.Children.Add(okButton);
                                buttonPanel.Children.Add(cancelButton);
                                panel.Children.Add(label);
                                panel.Children.Add(comboBox);
                                panel.Children.Add(buttonPanel);
                                roleDialog.Content = panel;

                                int? selectedRoleId = null;
                                okButton.Click += (s, e) =>
                                {
                                        selectedRoleId = ((Role)comboBox.SelectedItem).RoleId;
                                        roleDialog.DialogResult = true;
                                };
                                cancelButton.Click += (s, e) => roleDialog.DialogResult = false;

                                if (roleDialog.ShowDialog() != true)
                                        return;

                                // Generate a temporary password
                                string password = _securityService.GenerateStrongPassword();

                                _userRepository.Create(username, password, email, selectedRoleId.Value);
                                _auditLogRepository.LogAction(
                                    SessionManager.CurrentUser.UserId,
                                    "User_Created",
                                    $"New user created: {username} with role {AvailableRoles.First(r => r.RoleId == selectedRoleId.Value).RoleName}",
                                    "localhost");
                                LoadData();
                                LoadData(); // Refresh the users list

                                _dialogService.ShowMessage(
                                    $"User created successfully!\n\n" +
                                    $"Username: {username}\n" +
                                    $"Role: {AvailableRoles.First(r => r.RoleId == selectedRoleId.Value).RoleName}\n" +
                                    $"Temporary password: {password}\n\n" +
                                    "Please share this password securely with the user.");
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to create user: " + ex.Message);
                        }
                }
        }
}
