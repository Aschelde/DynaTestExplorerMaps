using DynaTestExplorerMaps.ViewModels;
using DynaTestExplorerMaps.Views;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;

namespace DynaTestExplorerMaps
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<MainWindow>();
            }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            //Authentication for ArcGIS location services
            try
            {
                // Initialize the ArcGIS Maps SDK runtime before any components are created.
                ArcGISRuntimeEnvironment.Initialize(config => config
                .UseLicense("runtimelite,1000,rud8290624519,none,E9PJD4SZ8LP7LMZ59172")
                .UseApiKey("AAPK05941c697899421c86df55261f04684cghLH1EjsYnL5YSkn2AukEOaRqixQryZAp9v5Av3VDPRtHHlQttyG_le3zufHiP9M")
                  .ConfigureAuthentication(auth => auth
                     .UseDefaultChallengeHandler() // Use the default authentication dialog
                  // .UseOAuthAuthorizeHandler(myOauthAuthorizationHandler) // Configure a custom OAuth dialog
                   )
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ArcGIS Maps SDK runtime initialization failed.");

                // Exit application
                this.Shutdown();
            }

            await AppHost!.StartAsync();
            var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
            startupForm!.DataContext = new MainWindowViewModel();
            startupForm!.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}
