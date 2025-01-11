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

namespace PasswordManager.App.ViewModels
{
        public class PasswordEntryViewModel : ViewModelBase
        {
                private readonly IStoredPasswordRepository _passwordRepository;
                private readonly ISecurityService _securityService;
                private readonly IDialogService _dialogService;
                private readonly IEncryptionService _encryptionService;
                private readonly IPasswordStrengthService _passwordStrengthService;
                private DateTime? _expirationDate;
                private PasswordStrength _passwordStrength;

                private int? _passwordId;
                private string _siteName;
                private string _siteUrl;
                private string _username;
                private string _password;
                private string _notes;
                private bool _isEditing;
                private bool _showPassword;

                public string SiteName
                {
                        get { return _siteName; }
                        set { SetProperty(ref _siteName, value); }
                }

                public string SiteUrl
                {
                        get { return _siteUrl; }
                        set { SetProperty(ref _siteUrl, value); }
                }

                public string Username
                {
                        get { return _username; }
                        set { SetProperty(ref _username, value); }
                }

                public string Password
                {
                        get { return _password; }
                        set { SetProperty(ref _password, value); }
                }

                public string Notes
                {
                        get { return _notes; }
                        set { SetProperty(ref _notes, value); }
                }

                public bool ShowPassword
                {
                        get { return _showPassword; }
                        set
                        {
                                if (SetProperty(ref _showPassword, value))
                                {
                                        OnPropertyChanged("DisplayPassword");
                                }
                        }
                }

                public string DisplayPassword
                {
                        get { return ShowPassword ? Password : new string('•', Password?.Length ?? 0); }
                }

                public bool IsEditing
                {
                        get { return _isEditing; }
                        set { SetProperty(ref _isEditing, value); }
                }

                public ICommand SaveCommand { get; private set; }
                public ICommand GeneratePasswordCommand { get; private set; }
                public ICommand CancelCommand { get; private set; }
                public ICommand TogglePasswordCommand { get; private set; }

                public event EventHandler RequestClose;

                public PasswordEntryViewModel(
                    IStoredPasswordRepository passwordRepository,
                    ISecurityService securityService,
                    IEncryptionService encryptionService,
                    IDialogService dialogService,
                    StoredPasswordModel existingPassword = null)
                {
                        _passwordRepository = passwordRepository;
                        _securityService = securityService;
                        _encryptionService = encryptionService;
                        _dialogService = dialogService;

                        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
                        GeneratePasswordCommand = new RelayCommand(ExecuteGeneratePassword);
                        CancelCommand = new RelayCommand(ExecuteCancel);
                        TogglePasswordCommand = new RelayCommand(ExecuteTogglePassword);

                        if (existingPassword != null)
                        {
                                LoadExistingPassword(existingPassword);
                                IsEditing = true;
                        }
                }

                private void LoadExistingPassword(StoredPasswordModel password)
                {
                        _passwordId = password.Id;
                        SiteName = password.SiteName;
                        SiteUrl = password.SiteUrl;
                        Username = password.Username;
                        Password = _encryptionService.Decrypt(password.EncryptedPassword);
                        Notes = password.Notes;
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
                                var passwordEntry = new StoredPasswordModel
                                {
                                        Id = _passwordId ?? 0,
                                        UserId = SessionManager.CurrentUser.Id,
                                        SiteName = SiteName,
                                        SiteUrl = SiteUrl,
                                        Username = Username,
                                        EncryptedPassword = _encryptionService.Encrypt(Password),
                                        Notes = Notes,
                                        ExpirationDate = ExpirationDate
                                };

                                if (IsEditing)
                                {
                                        _passwordRepository.Update(passwordEntry);
                                }
                                else
                                {
                                        _passwordRepository.Create(passwordEntry);
                                }

                                RequestClose?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError($"Failed to save password: {ex.Message}");
                        }
                }

                private void ExecuteGeneratePassword(object parameter)
                {
                        Password = _securityService.GenerateStrongPassword();
                        OnPropertyChanged("DisplayPassword");
                }

                private void ExecuteCancel(object parameter)
                {
                        RequestClose?.Invoke(this, EventArgs.Empty);
                }

                private void ExecuteTogglePassword(object parameter)
                {
                        ShowPassword = !ShowPassword;
                }

                public DateTime? ExpirationDate
                {
                        get { return _expirationDate; }
                        set { SetProperty(ref _expirationDate, value); }
                }

                public PasswordStrength PasswordStrength
                {
                        get { return _passwordStrength; }
                        private set
                        {
                                if (SetProperty(ref _passwordStrength, value))
                                {
                                        OnPropertyChanged("StrengthDescription");
                                        OnPropertyChanged("StrengthColor");
                                }
                        }
                }

                public string StrengthDescription
                {
                        get { return _passwordStrengthService.GetStrengthDescription(PasswordStrength); }
                }

                public string StrengthColor
                {
                        get { return _passwordStrengthService.GetStrengthColor(PasswordStrength); }
                }

                public ICommand CopyUsernameCommand { get; private set; }
                public ICommand CopyPasswordCommand { get; private set; }
                public ICommand DeleteCommand { get; private set; }

                public PasswordEntryViewModel(/* existing parameters */,
                    IPasswordStrengthService passwordStrengthService)
                {
                        _passwordStrengthService = passwordStrengthService;

                        // Add new commands
                        CopyUsernameCommand = new RelayCommand(ExecuteCopyUsername);
                        CopyPasswordCommand = new RelayCommand(ExecuteCopyPassword);
                        DeleteCommand = new RelayCommand(ExecuteDelete);

                        // Set default expiration
                        ExpirationDate = DateTime.Now.AddMonths(3);
                }

                protected override void OnPropertyChanged(string propertyName)
                {
                        base.OnPropertyChanged(propertyName);

                        if (propertyName == "Password")
                        {
                                PasswordStrength = _passwordStrengthService.CheckStrength(Password);
                        }
                }

                private void ExecuteCopyUsername(object parameter)
                {
                        try
                        {
                                Clipboard.SetText(Username);
                                _dialogService.ShowMessage("Username copied to clipboard!");
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to copy username: " + ex.Message);
                        }
                }

                private void ExecuteCopyPassword(object parameter)
                {
                        try
                        {
                                Clipboard.SetText(Password);
                                _dialogService.ShowMessage("Password copied to clipboard!");

                                // Clear clipboard after 30 seconds
                                Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(t =>
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

                private void ExecuteDelete(object parameter)
                {
                        if (!_dialogService.ShowConfirmation("Are you sure you want to delete this password?"))
                                return;

                        try
                        {
                                _passwordRepository.Delete(_passwordId.Value);
                                RequestClose?.Invoke(this, EventArgs.Empty);
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to delete password: " + ex.Message);
                        }
                }
        }
}