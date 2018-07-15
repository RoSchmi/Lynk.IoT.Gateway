using Lynk.IoT.Gateway.Contracts.Interfaces;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Prism.Windows.Validation;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Lynk.IoT.Gateway.ViewModels
{
    public class SettingsPageViewModel : ValidatableBindableBase
    {

        private readonly ISettingsService _settingsService;
        private readonly INetworkService _networkService;

        private int _port;

        [Required, Range(10000, 20000)]
        public int Port
        {
            get { return _port; }
            set
            {
                SetProperty(ref _port, value);
                Errors.Errors.TryGetValue("Port", out ReadOnlyCollection<string> errors);
                if (errors == null)
                {
                    _settingsService.SetInt("Port", value);
                    _networkService.Stop();
                    _networkService.Start();
                }
            }
        }


        public SettingsPageViewModel(ISettingsService settingsService, INetworkService networkService)
        {
            _settingsService = settingsService;
            _networkService = networkService;
            Port = _settingsService.GetInt("Port");
        }
    }
}
