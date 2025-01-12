using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Core.Services;
using PasswordManager.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PasswordManager.App.ViewModels
{
        public class ChangePasswordViewModel : ViewModelBase
        {
                private readonly IUserRepository _userRepository;
                private readonly ISecurityService _securityService;
                private readonly IPasswordStrengthService _passwordStrengthService;
                private readonly IDialogService _dialogService;

                private string _currentPassword;
                private string _newPassword;
                private string _confirmPassword;
                private PasswordStrength _passwordStrength;

                public string CurrentPassword
                {
                        get => _currentPassword;
                        set => SetProperty(ref _currentPassword, value);
                }

                public string NewPassword
                {
                        get => _newPassword;
                        set
                        {
                                if (SetProperty(ref _newPassword, value))
                                {
                                        _passwordStrength = _passwordStrengthService.CheckStrength(value);
                                        OnPropertyChanged(nameof(PasswordStrength));
                                        OnPropertyChanged(nameof(StrengthDescription));
                                        OnPropertyChanged(nameof(StrengthColor));
                                        ((RelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
                                }
                        }
                }

                public string ConfirmPassword
                {
                        get => _confirmPassword;
                        set
                        {
                                if (SetProperty(ref _confirmPassword, value))
                                {
                                        ((RelayCommand)ChangePasswordCommand).RaiseCanExecuteChanged();
                                }
                        }
                }

                public PasswordStrength PasswordStrength => _passwordStrength;
                public string StrengthDescription => _passwordStrengthService.GetStrengthDescription(_passwordStrength);
                public string StrengthColor => _passwordStrengthService.GetStrengthColor(_passwordStrength);

                public ICommand ChangePasswordCommand { get; }
                public ICommand CancelCommand { get; }
                public ICommand GeneratePasswordCommand { get; }

                public event EventHandler RequestClose;

                public ChangePasswordViewModel(
                    IUserRepository userRepository,
                    ISecurityService securityService,
                    IPasswordStrengthService passwordStrengthService,
                    IDialogService dialogService)
                {
                        _userRepository = userRepository;
                        _securityService = securityService;
                        _passwordStrengthService = passwordStrengthService;
                        _dialogService = dialogService;

                        ChangePasswordCommand = new RelayCommand(ExecuteChangePassword, CanExecuteChangePassword);
                        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
                        GeneratePasswordCommand = new RelayCommand(_ => ExecuteGeneratePassword());
                }

                private bool CanExecuteChangePassword(object parameter)
                {
                        return !string.IsNullOrWhiteSpace(CurrentPassword) &&
                               !string.IsNullOrWhiteSpace(NewPassword) &&
                               !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                               NewPassword == ConfirmPassword &&
                               _passwordStrength >= PasswordStrength.Medium;
                }

                private void ExecuteChangePassword(object parameter)
                {
                        try
                        {
                                // Verify current password
                                if (!_userRepository.ValidateUser(
                                    Core.Services.SessionManager.CurrentUser.Username,
                                    CurrentPassword))
                                {
                                        SetError("Current password is incorrect");
                                        return;
                                }

                                // Change password
                                _userRepository.ChangePassword(
                                    Core.Services.SessionManager.CurrentUser.UserId,
                                    NewPassword);

                                _dialogService.ShowMessage("Password changed successfully!");
                                RequestClose?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                                SetError("Failed to change password: " + ex.Message);
                        }
                }

                private void ExecuteGeneratePassword()
                {
                        NewPassword = _securityService.GenerateStrongPassword();
                        ConfirmPassword = NewPassword;
                }
        }
}
