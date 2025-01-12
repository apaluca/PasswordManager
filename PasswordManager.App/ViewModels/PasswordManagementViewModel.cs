using PasswordManager.App.Views;
using PasswordManager.Core.Models;
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
using System.Windows;

namespace PasswordManager.App.ViewModels
{
        public class PasswordManagementViewModel : ViewModelBase
        {
                private readonly IStoredPasswordRepository _passwordRepository;
                private readonly ISecurityService _securityService;
                private readonly IEncryptionService _encryptionService;
                private readonly IDialogService _dialogService;
                private readonly IPasswordStrengthService _passwordStrengthService;

                public ObservableCollection<StoredPasswordModel> Passwords { get; private set; }

                private StoredPasswordModel _selectedPassword;
                private string _searchText;
                private bool _showExpiredOnly;

                public StoredPasswordModel SelectedPassword
                {
                        get { return _selectedPassword; }
                        set
                        {
                                SetProperty(ref _selectedPassword, value);
                                ((RelayCommand)EditPasswordCommand).RaiseCanExecuteChanged();
                                ((RelayCommand)DeletePasswordCommand).RaiseCanExecuteChanged();
                        }
                }

                public string SearchText
                {
                        get { return _searchText; }
                        set
                        {
                                SetProperty(ref _searchText, value);
                                FilterPasswords();
                        }
                }

                public bool ShowExpiredOnly
                {
                        get { return _showExpiredOnly; }
                        set
                        {
                                SetProperty(ref _showExpiredOnly, value);
                                FilterPasswords();
                        }
                }

                public ICommand AddPasswordCommand { get; private set; }
                public ICommand EditPasswordCommand { get; private set; }
                public ICommand DeletePasswordCommand { get; private set; }
                public ICommand RefreshCommand { get; private set; }
                public ICommand CopyUsernameCommand { get; private set; }
                public ICommand CopyPasswordCommand { get; private set; }

                public PasswordManagementViewModel(
                    IStoredPasswordRepository passwordRepository,
                    ISecurityService securityService,
                    IEncryptionService encryptionService,
                    IDialogService dialogService,
                    IPasswordStrengthService passwordStrengthService)
                {
                        _passwordRepository = passwordRepository;
                        _securityService = securityService;
                        _encryptionService = encryptionService;
                        _dialogService = dialogService;
                        _passwordStrengthService = passwordStrengthService;

                        Passwords = new ObservableCollection<StoredPasswordModel>();

                        AddPasswordCommand = new RelayCommand(_ => ExecuteAddPassword());
                        EditPasswordCommand = new RelayCommand(_ => ExecuteEditPassword(), _ => CanExecutePasswordAction());
                        DeletePasswordCommand = new RelayCommand(_ => ExecuteDeletePassword(), _ => CanExecutePasswordAction());
                        RefreshCommand = new RelayCommand(_ => LoadPasswords());
                        CopyUsernameCommand = new RelayCommand(ExecuteCopyUsername);
                        CopyPasswordCommand = new RelayCommand(ExecuteCopyPassword);

                        LoadPasswords();
                }

                private void LoadPasswords()
                {
                        try
                        {
                                var passwords = _passwordRepository
                                    .GetByUserId(SessionManager.CurrentUser.UserId)
                                    .Select(p => new StoredPasswordModel
                                    {
                                            Id = p.PasswordId,
                                            UserId = p.UserId,
                                            SiteName = p.SiteName,
                                            SiteUrl = p.SiteUrl,
                                            Username = p.Username,
                                            EncryptedPassword = p.EncryptedPassword,
                                            Notes = p.Notes,
                                            ExpirationDate = p.CreatedDate?.AddMonths(3), // Example expiration logic
                                            CreatedDate = p.CreatedDate,
                                            ModifiedDate = p.ModifiedDate
                                    });

                                Passwords.Clear();
                                foreach (var password in passwords)
                                {
                                        Passwords.Add(password);
                                }

                                FilterPasswords();
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to load passwords: " + ex.Message);
                        }
                }

                private void FilterPasswords()
                {
                        IEnumerable<StoredPasswordModel> filtered = _passwordRepository
                            .GetByUserId(SessionManager.CurrentUser.UserId)
                            .Select(p => new StoredPasswordModel
                            {
                                    Id = p.PasswordId,
                                    UserId = p.UserId,
                                    SiteName = p.SiteName,
                                    SiteUrl = p.SiteUrl,
                                    Username = p.Username,
                                    EncryptedPassword = p.EncryptedPassword,
                                    Notes = p.Notes,
                                    ExpirationDate = p.CreatedDate?.AddMonths(3),
                                    CreatedDate = p.CreatedDate,
                                    ModifiedDate = p.ModifiedDate
                            });

                        if (!string.IsNullOrWhiteSpace(SearchText))
                        {
                                string search = SearchText.ToLower();
                                filtered = filtered.Where(p =>
                                    p.SiteName.ToLower().Contains(search) ||
                                    p.Username.ToLower().Contains(search) ||
                                    p.SiteUrl?.ToLower().Contains(search) == true);
                        }

                        if (ShowExpiredOnly)
                        {
                                filtered = filtered.Where(p =>
                                    p.ExpirationDate.HasValue &&
                                    p.ExpirationDate.Value < DateTime.Now);
                        }

                        Passwords.Clear();
                        foreach (var password in filtered)
                        {
                                Passwords.Add(password);
                        }
                }

                private bool CanExecutePasswordAction()
                {
                        return SelectedPassword != null;
                }

                private void ExecuteAddPassword()
                {
                        var viewModel = new PasswordEntryViewModel(
                            _passwordRepository,
                            _securityService,
                            _encryptionService,
                            _dialogService,
                            _passwordStrengthService);

                        var window = new PasswordEntryWindow
                        {
                                Owner = Application.Current.MainWindow,
                                DataContext = viewModel
                        };

                        viewModel.RequestClose += (s, e) =>
                        {
                                window.DialogResult = true;
                                LoadPasswords();
                        };

                        window.ShowDialog();
                }

                private void ExecuteEditPassword()
                {
                        var viewModel = new PasswordEntryViewModel(
                            _passwordRepository,
                            _securityService,
                            _encryptionService,
                            _dialogService,
                            _passwordStrengthService,
                            SelectedPassword);

                        var window = new PasswordEntryWindow
                        {
                                Owner = Application.Current.MainWindow,
                                DataContext = viewModel
                        };

                        viewModel.RequestClose += (s, e) =>
                        {
                                window.DialogResult = true;
                                LoadPasswords();
                        };

                        window.ShowDialog();
                }

                private void ExecuteDeletePassword()
                {
                        if (_dialogService.ShowConfirmation(
                            $"Are you sure you want to delete the password for {SelectedPassword.SiteName}?"))
                        {
                                try
                                {
                                        _passwordRepository.Delete(SelectedPassword.Id);
                                        LoadPasswords();
                                        _dialogService.ShowMessage("Password deleted successfully.");
                                }
                                catch (Exception ex)
                                {
                                        _dialogService.ShowError("Failed to delete password: " + ex.Message);
                                }
                        }
                }

                private void ExecuteCopyUsername(object parameter)
                {
                        var password = parameter as StoredPasswordModel;
                        if (password != null)
                        {
                                try
                                {
                                        Clipboard.SetText(password.Username);
                                        _dialogService.ShowMessage("Username copied to clipboard!");

                                        // Clear clipboard after 30 seconds
                                        Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(t =>
                                        {
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                        if (Clipboard.GetText() == password.Username)
                                                        {
                                                                Clipboard.Clear();
                                                        }
                                                });
                                        });
                                }
                                catch (Exception ex)
                                {
                                        _dialogService.ShowError("Failed to copy username: " + ex.Message);
                                }
                        }
                }

                private void ExecuteCopyPassword(object parameter)
                {
                        var password = parameter as StoredPasswordModel;
                        if (password != null)
                        {
                                try
                                {
                                        string decryptedPassword = _encryptionService.Decrypt(password.EncryptedPassword);
                                        Clipboard.SetText(decryptedPassword);
                                        _dialogService.ShowMessage("Password copied to clipboard!");

                                        // Clear clipboard after 30 seconds
                                        Task.Delay(TimeSpan.FromSeconds(30)).ContinueWith(t =>
                                        {
                                                Application.Current.Dispatcher.Invoke(() =>
                                                {
                                                        if (Clipboard.GetText() == decryptedPassword)
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
                }
        }
}
