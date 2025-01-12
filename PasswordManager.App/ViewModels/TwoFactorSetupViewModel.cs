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
using System.Windows.Media.Imaging;
using System.IO;
using System.Net.Http;
using PasswordManager.Data.Repositories;

namespace PasswordManager.App.ViewModels
{
        public class TwoFactorSetupViewModel : ViewModelBase
        {
                private readonly ISecurityService _securityService;
                private readonly IUserRepository _userRepository;
                private readonly IDialogService _dialogService;
                private readonly IAuditLogRepository _auditLogRepository;

                private string _setupKey;
                private string _verificationCode;
                private string _qrCodeUri;
                private int _remainingSeconds;
                private bool _isVerified;
                private BitmapImage _qrCodeImage;
                public BitmapImage QrCodeImage
                {
                        get { return _qrCodeImage; }
                        private set { SetProperty(ref _qrCodeImage, value); }
                }

                public string SetupKey
                {
                        get { return _setupKey; }
                        private set { SetProperty(ref _setupKey, value); }
                }

                public string VerificationCode
                {
                        get { return _verificationCode; }
                        set
                        {
                                SetProperty(ref _verificationCode, value);
                                ((RelayCommand)VerifyCommand).RaiseCanExecuteChanged();
                        }
                }

                public string QrCodeUri
                {
                        get { return _qrCodeUri; }
                        private set { SetProperty(ref _qrCodeUri, value); }
                }

                public int RemainingSeconds
                {
                        get { return _remainingSeconds; }
                        private set { SetProperty(ref _remainingSeconds, value); }
                }

                public bool IsVerified
                {
                        get { return _isVerified; }
                        private set { SetProperty(ref _isVerified, value); }
                }

                public ICommand VerifyCommand { get; private set; }
                public ICommand GenerateNewKeyCommand { get; private set; }

                public event EventHandler SetupCompleted;

                public TwoFactorSetupViewModel(
                    ISecurityService securityService,
                    IUserRepository userRepository,
                    IDialogService dialogService,
                    IAuditLogRepository auditLogRepository)
                {
                        _securityService = securityService;
                        _userRepository = userRepository;
                        _dialogService = dialogService;
                        _auditLogRepository = auditLogRepository;

                        VerifyCommand = new RelayCommand(ExecuteVerify, CanExecuteVerify);
                        GenerateNewKeyCommand = new RelayCommand(_ => GenerateNewKey());

                        // Generate initial key
                        GenerateNewKey();

                        // Start timer to update remaining seconds
                        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += (s, e) => UpdateRemainingSeconds();
                        timer.Start();
                }

                private void GenerateNewKey()
                {
                        SetupKey = _securityService.GenerateTwoFactorKey();
                        QrCodeUri = _securityService.GetTwoFactorQrCodeUri(
                            SetupKey,
                            SessionManager.CurrentUser.Username);
                        LoadQrCode();
                        IsVerified = false;
                        VerificationCode = string.Empty;
                }

                private bool CanExecuteVerify(object parameter)
                {
                        return !string.IsNullOrWhiteSpace(VerificationCode) &&
                               VerificationCode.Length == 6 &&
                               !IsVerified;
                }

                private void ExecuteVerify(object parameter)
                {
                        if (_securityService.ValidateTwoFactorCode(SetupKey, VerificationCode))
                        {
                                try
                                {
                                        _userRepository.EnableTwoFactor(
                                            SessionManager.CurrentUser.UserId,
                                        SetupKey);

                                        _auditLogRepository.LogAction(
                                            SessionManager.CurrentUser.UserId,
                                            "Security_2FAEnabled",
                                            "Two-factor authentication enabled successfully",
                                            "localhost");

                                        IsVerified = true;
                                        _dialogService.ShowMessage(
                                            "Two-factor authentication has been successfully enabled!");

                                        SetupCompleted?.Invoke(this, EventArgs.Empty);
                                }
                                catch (Exception ex)
                                {
                                        _dialogService.ShowError(
                                            "Failed to enable two-factor authentication: " + ex.Message);
                                }
                        }
                        else
                        {
                                _auditLogRepository.LogAction(
                                    SessionManager.CurrentUser.UserId,
                                    "Security_2FAVerificationFailed",
                                    "Failed verification attempt for 2FA setup",
                                    "localhost");
                                _dialogService.ShowError(
                                    "Invalid verification code. Please try again.");
                                VerificationCode = string.Empty;
                        }
                }

                private void UpdateRemainingSeconds()
                {
                        RemainingSeconds = _securityService.GetRemainingTotpSeconds();
                }

                private async void LoadQrCode()
                {
                        try
                        {
                                using (var client = new HttpClient())
                                {
                                        string url = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(QrCodeUri)}";
                                        var bytes = await client.GetByteArrayAsync(url);

                                        var image = new BitmapImage();
                                        using (var mem = new MemoryStream(bytes))
                                        {
                                                image.BeginInit();
                                                image.CacheOption = BitmapCacheOption.OnLoad;
                                                image.StreamSource = mem;
                                                image.EndInit();
                                        }
                                        image.Freeze(); // Important for cross-thread usage
                                        QrCodeImage = image;
                                }
                        }
                        catch (Exception ex)
                        {
                                _dialogService.ShowError("Failed to load QR code: " + ex.Message);
                        }
                }
        }
}
