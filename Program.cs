using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PasswordManager.Interfaces;
using PasswordManager.Services;
using System;
using System.Windows.Forms;

namespace PasswordManager
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.Run(ServiceProvider.GetRequiredService<Form1>());
        }
        public static IServiceProvider ServiceProvider { get; private set; }

        static IHostBuilder CreateHostBuilder()
        {
            var environmentService = new EnvironmentService();
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddTransient<IPasswordService, PasswordService>();
                    services.AddTransient<IEncryptionService, EncryptionService>();
                    if(environmentService.GetEnvironmentVariable(environmentService.PgDBType).ToLower() == "xml")
                    {
                        services.AddTransient<IDataBaseService, DataBaseXMLService>();
                    }
                    else
                    {
                        services.AddTransient<IDataBaseService, DataBaseJSONService>();
                    }                    
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<Form1>();
                });
        }
    }
}
