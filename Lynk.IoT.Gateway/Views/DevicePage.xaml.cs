using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Lynk.IoT.Gateway.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DevicePage : Page
    {
        public DevicePage()
        {
            this.InitializeComponent();
        }

        private void btnAdd_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            devicesList.SelectedIndex = -1;
        }
    }
}
