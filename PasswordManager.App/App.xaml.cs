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

namespace PasswordManager.App
{
        /// <summary>
        /// Interaction logic for App.xaml
        /// </summary>
        public partial class App : Application
        {
                private ServiceProvider _serviceProvider;

                public App()
                {
                        var encryptionService = new EncryptionService(null);
                        string key = encryptionService.GenerateKey();
                        // The key will be printed in your debug output
                        //System.Diagnostics.Debug.WriteLine($"Generated Key: {key}");
                        //var services = new ServiceCollection();
                        //ConfigureServices(services);
                        //_serviceProvider = services.BuildServiceProvider();
                }

                private void ConfigureServices(IServiceCollection services)
                {
                        // Load configuration
                        IConfiguration configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .Build();

                        // Add services
                        services.AddPasswordManagerServices(configuration);
                        services.AddSingleton<IDialogService, DialogService>();
                        services.AddSingleton<INavigationService, NavigationService>();

                        // Register ViewModels
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<PasswordManagementViewModel>();
                        services.AddTransient<UserManagementViewModel>();
                        services.AddTransient<SettingsViewModel>();

                        // Register Views
                        services.AddTransient<LoginWindow>();
                        services.AddTransient<MainWindow>();
                }

                protected override void OnStartup(StartupEventArgs e)
                {
                        base.OnStartup(e);

                        var loginWindow = _serviceProvider.GetService<LoginWindow>();
                        Application.Current.MainWindow = loginWindow;
                        loginWindow.Show();
                }

                protected override void OnExit(ExitEventArgs e)
                {
                        base.OnExit(e);
                        _serviceProvider?.Dispose();
                }
        }
}
