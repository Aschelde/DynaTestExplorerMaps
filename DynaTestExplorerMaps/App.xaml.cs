using DynaTestExplorerMaps.ViewModels;
using DynaTestExplorerMaps.Views;
using DynaTestExplorerMaps.Interfaces;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using DynaTestExplorerMaps.DataAccess;
using DynaTestExplorerMaps.Services;

namespace DynaTestExplorerMaps
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            InitializeArcGISRuntimeEnvironment();

            AppHost = Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IImageLoader, ImageLoader>();
                services.AddSingleton<IDataAccessLayer, DataAccessLayer>();

                services.AddSingleton<IMapService, MapService>();

                services.AddSingleton<MainWindow>();
                services.AddTransient<IMapViewModel, MapViewModel>();
                services.AddTransient<IImageViewModel, ImageViewModel>();
                services.AddTransient<IDataViewModel, DataViewModel>();
                services.AddTransient<IOptionsViewModel ,OptionsViewModel>();

                services.AddTransient<MapControl>();
                services.AddTransient<ImageControl>();
                services.AddTransient<DataControl>();
                services.AddTransient<OptionsControl>();
            }).Build();

            ServiceProvider = AppHost.Services;

            await AppHost!.StartAsync();
            var startupForm = ServiceProvider.GetRequiredService<MainWindow>();
            startupForm.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }

        private void InitializeArcGISRuntimeEnvironment()
        {
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
        }
    }
}
