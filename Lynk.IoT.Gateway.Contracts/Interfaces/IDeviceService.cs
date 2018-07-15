using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lynk.IoT.Gateway.Contracts.Interfaces
{
    public interface IDeviceService
    {
        ObservableCollection<ViewModels.DeviceInfo> Connected { get; }
      
        Task AddAsync(ViewModels.DeviceInfo device);

        Task RemoveAsync(ViewModels.DeviceInfo device);

        Task<ViewModels.DeviceInfo> AuthenticateAsync(Guid id, string key);

        Task<ViewModels.DeviceInfo> GetAsync(Guid id);
        
        Task<List<ViewModels.DeviceInfo>> GetAsync();

        Task UpdateAsync(ViewModels.DeviceInfo device);

        Task ProcessIncomingAsync(ViewModels.DeviceInfo device, string data);
    }
}
