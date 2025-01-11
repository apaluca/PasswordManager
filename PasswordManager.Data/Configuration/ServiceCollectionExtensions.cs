using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Core.Services.Interfaces;
using PasswordManager.Core.Services;
using PasswordManager.Data.DataContext;
using PasswordManager.Data.Repositories.Interfaces;
using PasswordManager.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Data.Configuration
{
        public static class ServiceCollectionExtensions
        {
                public static IServiceCollection AddPasswordManagerServices(
                    this IServiceCollection services,
                    IConfiguration configuration)
                {
                        // Bind configuration
                        var appSettings = new AppSettings();
                        configuration.Bind(appSettings);
                        services.AddSingleton(appSettings);

                        // Register DataContext
                        services.AddScoped<PasswordManagerDataContext>(_ =>
                            new PasswordManagerDataContext(appSettings.ConnectionString));

                        // Register Services
                        services.AddSingleton<IEncryptionService>(_ =>
                            new EncryptionService(appSettings.EncryptionKey));
                        services.AddSingleton<ISecurityService, SecurityService>();

                        // Register Repositories
                        services.AddScoped<IUserRepository, UserRepository>();
                        services.AddScoped<IStoredPasswordRepository, StoredPasswordRepository>();
                        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();

                        return services;
                }
        }
}
