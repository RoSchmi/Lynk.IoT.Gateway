using Prism.Windows.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Lynk.IoT.Gateway.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public  Frame RootFrame { get; }
        private INavigationService _navigationService;

        public Shell(Frame rootFrame, INavigationService navigationService)
        {
            this.RootFrame = rootFrame;
            this._navigationService = navigationService;
            this.InitializeComponent();
            Window.Current.SetTitleBar(appTitleContentPresenter);
        }

        private void navMenu_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            _navigationService.ClearHistory();
            _navigationService.RemoveAllPages();
            if (args.IsSettingsSelected)
            {
                _navigationService.Navigate("Settings", null);
            }
            else
            {
                string page = ((NavigationViewItem)args.SelectedItem).Tag.ToString();
                _navigationService.Navigate(page, null);
            }

            _navigationService.ClearHistory();
        }
    }
}
