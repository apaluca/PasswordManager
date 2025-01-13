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
using System.Windows;
using PasswordManager.Core.Models;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories;

namespace PasswordManager.App.ViewModels
{
        public class PasswordEntryViewModel : ViewModelBase
        {
                private readonly IStoredPasswordRepository _passwordRepository;
                private readonly ISecurityService _securityService;
                private readonly IDialogService _dialogService;
                private readonly IEncryptionService _encryptionService;
                private readonly IPasswordStrengthService _passwordStrengthService;
                private readonly IAuditLogRepository _auditLogRepository;

                private int? _passwordId;
                private string _siteName;
                private string _siteUrl;
                private string _username;
                private string _password;
                private string _notes;
                private DateTime? _expirationDate;
                private PasswordStrength _passwordStrength;
                private bool _showPassword;
                private bool _isEditing;
                private bool _hasExpirationDate;

                public string SiteName
                {
                        get => _siteName;
                        set
                        {
                                if (SetProperty(ref _siteName, value))
                                {
                                        ValidateInput();
                                }
                        }
                }

                public string SiteUrl
                {
                        get => _siteUrl;
                        set => SetProperty(ref _siteUrl, value);
                }

                public string Username
                {
                        get => _username;
                        set
                        {
                                if (SetProperty(ref _username, value))
                                {
                                        ValidateInput();
                                }
                        }
                }

                public string Password
                {
                        get => _password;
                        set
                        {
                                if (SetProperty(ref _password, value))
                                {
                                        OnPropertyChanged(nameof(DisplayPassword));
                                        _passwordStrength = _passwordStrengthService.CheckStrength(value);
                                        OnPropertyChanged(nameof(PasswordStrength));
                                        OnPropertyChanged(nameof(StrengthDescription));
                                        OnPropertyChanged(nameof(StrengthColor));
                                        ValidateInput();
                                }
                        }
                }

                public string Notes
                {
                        get => _notes;
                        set => SetProperty(ref _notes, value);
                }

                public DateTime? ExpirationDate
                {
                        get => _expirationDate;
                        set => SetProperty(ref _expirationDate, value);
                }

                public bool ShowPassword
                {
                        get => _showPassword;
                        set
                        {
                                if (SetProperty(ref _showPassword, value))
                                {
                                        OnPropertyChanged(nameof(DisplayPassword));
                                }
                        }
                }

                public string DisplayPassword => new string('•', Password?.Length ?? 0);

                public bool IsEditing
                {
                        get => _isEditing;
                        private set => SetProperty(ref _isEditing, value);
                }

                public bool HasExpirationDate
                {
                        get => _hasExpirationDate;
                        set
                        {
                                if (SetProperty(ref _hasExpirationDate, value))
                                {
                                        if (!value)
                                        {
                                                ExpirationDate = null;
                                        }
                                        else if (!ExpirationDate.HasValue)
                                        {
                                                ExpirationDate = DateTime.Now.AddMonths(3);
                                        }
                                }
                        }
                }

                public PasswordStrength PasswordStrength => _passwordStrength;

                public string StrengthDescription => _passwordStrengthService.GetStrengthDescription(_passwordStrength);

                public string StrengthColor => _passwordStrengthService.GetStrengthColor(_passwordStrength);

                public ICommand SaveCommand { get; }
                public ICommand GeneratePasswordCommand { get; }
                public ICommand CancelCommand { get; }
                public ICommand TogglePasswordCommand { get; }
                public ICommand CopyPasswordCommand { get; }

                public event EventHandler RequestClose;

                public PasswordEntryViewModel(
                    IStoredPasswordRepository passwordRepository,
                    ISecurityService securityService,
                    IEncryptionService encryptionService,
                    IDialogService dialogService,
                    IPasswordStrengthService passwordStrengthService,
                    IAuditLogRepository auditLogRepository,
                    StoredPasswordModel existingPassword = null)
                {
                        _passwordRepository = passwordRepository;
                        _securityService = securityService;
                        _encryptionService = encryptionService;
                        _dialogService = dialogService;
                        _passwordStrengthService = passwordStrengthService;
                        _auditLogRepository = auditLogRepository;

                        if (existingPassword != null)
                        {
                                LoadExistingPassword(existingPassword);
                                IsEditing = true;
                        }

                        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
                        GeneratePasswordCommand = new RelayCommand(_ => ExecuteGeneratePassword());
                        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, EventArgs.Empty));
                        TogglePasswordCommand = new RelayCommand(_ => ShowPassword = !ShowPassword);
                        CopyPasswordCommand = new RelayCommand(_ => ExecuteCopyPassword());
                }

                private void LoadExistingPassword(StoredPasswordModel password)
                {
                        _passwordId = password.Id;
                        SiteName = password.SiteName;
                        SiteUrl = password.SiteUrl;
                        Username = password.Username;
                        Password = _encryptionService.Decrypt(password.EncryptedPassword);
                        Notes = password.Notes;
                        ExpirationDate = password.ExpirationDate;
                        HasExpirationDate = password.ExpirationDate.HasValue;
                }

                private bool CanExecuteSave(object parameter)
                {
                        return !string.IsNullOrWhiteSpace(SiteName) &&
                               !string.IsNullOrWhiteSpace(Username) &&
                               !string.IsNullOrWhiteSpace(Password);
                }


                private void ExecuteSave(object parameter)
                {
                        try
                        {
                                DateTime? createdDate;
                                if (_passwordId.HasValue)
                                {
                                        createdDate = null;
                                }
                                else
                                {
                                        createdDate = DateTime.Now;
                                }

                                var passwordEntry = new StoredPasswordModel
                                {
                                        Id = _passwordId ?? 0,
                                        UserId = SessionManager.CurrentUser.UserId,
                                        SiteName = SiteName,
                                        SiteUrl = SiteUrl,
                                        Username = Username,
                                        EncryptedPassword = _encryptionService.Encrypt(Password),
                                        Notes = Notes,
                                        CreatedDate = createdDate,
                                        ModifiedDate = DateTime.Now,
                                        ExpirationDate = HasExpirationDate ? ExpirationDate : null
                                };

                                if (IsEditing)
                                {
                                        _passwordRepository.Update(passwordEntry);
                                        _auditLogRepository.LogAction(
                                            SessionManager.CurrentUser.UserId,
                                            "User_PasswordUpdated",
                                            $"Updated stored password for site: {SiteName}",
                                            "localhost");
                                }
                                else
                                {
                                        _passwordRepository.Create(passwordEntry);
                                        _auditLogRepository.LogAction(
                                            SessionManager.CurrentUser.UserId,
                                            "User_PasswordCreated",
                                            $"Created new stored password for site: {SiteName}",
                                            "localhost");
                                }

                                RequestClose?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to save password: " + ex.Message);
                        }
                }

                private void ExecuteGeneratePassword()
                {
                        Password = _securityService.GenerateStrongPassword();
                        ShowPassword = true;
                }

                private void ExecuteCopyPassword()
                {
                        try
                        {
                                Clipboard.SetText(Password);
                                _dialogService.ShowMessage("Password copied to clipboard!");
                                _auditLogRepository.LogAction(
                                    SessionManager.CurrentUser.UserId,
                                    "Password_Copied",
                                    $"Copied password for {SiteName}",
                                    "localhost");

                                // Clear clipboard after 30 seconds
                                System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(30))
                                    .ContinueWith(t =>
                                    {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                    if (Clipboard.GetText() == Password)
                                                    {
                                                            Clipboard.Clear();
                                                    }
                                            });
                                    });
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to copy password: " + ex.Message);
                        }
                }

                private void ValidateInput()
                {
                        if (SaveCommand is RelayCommand relayCommand)
                        {
                                relayCommand.RaiseCanExecuteChanged();
                        }
                }
        }
}