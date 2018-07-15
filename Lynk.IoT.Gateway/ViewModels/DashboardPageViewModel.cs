using Lynk.IoT.Gateway.Contracts.Interfaces;
using Lynk.IoT.Gateway.Contracts.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lynk.IoT.Gateway.ViewModels
{
    public class DashboardPageViewModel : ViewModelBase
    {
        public ICommand RestartCommand { get; }
        public IDeviceService Service { get; }

        private readonly INetworkService _networkService;

        public DashboardPageViewModel(IDeviceService deviceService, INetworkService networkService)
        {
            Service = deviceService;
            _networkService = networkService;
            RestartCommand = new DelegateCommand(async () =>
            {
                await Task.Delay(1000);
                _networkService.Stop();
                await Task.Delay(1000);
                _networkService.Start();

            });
        }


    }
}
