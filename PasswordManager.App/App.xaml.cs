using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Core.MVVM.Interfaces;
using PasswordManager.Core.MVVM;
using PasswordManager.Core.Services;
using PasswordManager.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.App.Services;
using PasswordManager.App.ViewModels;
using PasswordManager.App.Views;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories.Interfaces;
using PasswordManager.Data.Repositories;

namespace PasswordManager.App
{
        public partial class App : Application
        {
                private ServiceProvider _serviceProvider;

                public App()
                {
                        //// TEMPORARY: Generate and show new encryption key
                        //// Note: You should comment out the loginWindow code in OnStartup before running this
                        //var tempService = new EncryptionService("temporary");
                        //string newKey = tempService.GenerateKey();
                        //var keyWindow = new EncryptionKeyWindow(newKey);
                        //keyWindow.ShowDialog();
                        //// END TEMPORARY

                        //// TEMPORARY: Generate hash for admin password
                        //var securityService = new SecurityService();
                        //var hash = securityService.GenerateHashForTest("admin123");
                        //var hashWindow = new EncryptionKeyWindow(hash);
                        //hashWindow.ShowDialog();
                        //// END TEMPORARY

                        ServiceCollection services = new ServiceCollection();
                        ConfigureServices(services);
                        _serviceProvider = services.BuildServiceProvider();
                }

                private void ConfigureServices(ServiceCollection services)
                {
                        // Configuration
                        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                        string encryptionKey = System.Configuration.ConfigurationManager.AppSettings["EncryptionKey"];

                        // Register DataContext
                        services.AddSingleton(new PasswordManagerDataContext(connectionString));

                        // Register Services
                        services.AddSingleton<IEncryptionService>(new EncryptionService(encryptionKey));
                        services.AddSingleton<ISecurityService, SecurityService>();
                        services.AddSingleton<IPasswordStrengthService, PasswordStrengthService>();
                        services.AddSingleton<IDialogService, DialogService>();
                        services.AddSingleton<INavigationService>(sp => new NavigationService(sp));

                        // Register Repositories
                        services.AddScoped<IUserRepository, UserRepository>();
                        services.AddScoped<IStoredPasswordRepository, StoredPasswordRepository>();
                        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();

                        // Register ViewModels
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<DashboardViewModel>();
                        services.AddTransient<PasswordManagementViewModel>();
                        services.AddTransient<AdminDashboardViewModel>();
                        services.AddTransient<ITDashboardViewModel>();
                        services.AddTransient<TwoFactorSetupViewModel>();

                        // Register Views
                        services.AddTransient<LoginWindow>();
                        services.AddTransient<MainWindow>();
                }

                protected override void OnStartup(StartupEventArgs e)
                {
                        base.OnStartup(e);

                        var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
                        var loginWindow = new LoginWindow(loginViewModel);
                        loginWindow.Show();
                        MainWindow = loginWindow;
                }

                protected override void OnExit(ExitEventArgs e)
                {
                        base.OnExit(e);
                        _serviceProvider?.Dispose();
                }
        }
}
