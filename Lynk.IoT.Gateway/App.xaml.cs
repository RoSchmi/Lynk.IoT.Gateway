using Lynk.IoT.Gateway.Contracts.Interfaces;
using Lynk.IoT.Gateway.Services;
using Prism.Unity.Windows;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Unity;
using Lynk.IoT.Gateway.Persistent;
using Windows.UI.Core;

namespace Lynk.IoT.Gateway
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            //Remove title bar
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            //remove the solid-colored backgrounds behind the caption controls and system back button
            ApplicationViewTitleBar viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            viewTitleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
            var networkService = ((NetworkServerService)Container.Resolve<INetworkService>()); ;
            networkService.Start();
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            NavigationService.Navigate("Dashboard", null);
            NavigationService.ClearHistory();
            NavigationService.RemoveAllPages();

            return Task.CompletedTask;
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = new Views.Shell(rootFrame, NavigationService);
            return shell;
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.RegisterInstance<CoreDispatcher>(Window.Current.Dispatcher);
            Container.RegisterSingleton<IDeviceService, DeviceService>();
            Container.RegisterSingleton<ISettingsService, SettingsService>();
            Container.RegisterSingleton<INetworkService, NetworkServerService>();
            Container.RegisterType<IRepository, Repository>();
        }
    }
}
