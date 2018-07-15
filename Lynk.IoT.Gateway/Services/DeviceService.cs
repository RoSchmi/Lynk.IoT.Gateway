using Lynk.IoT.Gateway.Contracts.Interfaces;
using Lynk.IoT.Gateway.Contracts.Models;
using Lynk.IoT.Gateway.Contracts.ViewModels;
using Lynk.IoT.Gateway.Persistent.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Lynk.IoT.Gateway.Services
{
    public class DeviceService : IDeviceService
    {
        public ObservableCollection<DeviceInfo> Connected { get; }

        private readonly IRepository _repository;
        private readonly CoreDispatcher _dispatcher;

        public DeviceService(IRepository repository, CoreDispatcher dispatcher)
        {
            Connected = new ObservableCollection<DeviceInfo>();
            _repository = repository;
            _dispatcher = dispatcher;
        }

        public async Task AddAsync(DeviceInfo device)
        {
            var entity = new Device { Key = device.Key, Name = device.Name, OS = device.OS };
            await _repository.AddEntityAsync<Device>(entity);
            device.Id = entity.Id;
        }

        public async Task RemoveAsync(DeviceInfo device)
        {
            var entity = _repository.Get<Device>().Where(x => x.Id == device.Id).FirstOrDefault();
            if (entity != null)
                await _repository.DeleteEntityAsync(entity);
        }

        public Task<DeviceInfo> AuthenticateAsync(Guid id, string key)
        {
            return Task.FromResult(_repository.Get<Device>().Where(x => x.Id == id && x.Key == key).Select(x => new DeviceInfo { Id = id, Key = key, Name = x.Name, OS = x.OS }).FirstOrDefault());
        }

        public Task<DeviceInfo> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(DeviceInfo device)
        {
            var entity = _repository.Get<Device>().Where(x => x.Id == device.Id).FirstOrDefault();
            entity.Key = device.Key;
            entity.Name = device.Name;
            entity.OS = device.OS;
            await _repository.UpdateEntityAsync<Device>(entity); ;
        }

        public async Task ProcessIncomingAsync(DeviceInfo device, string data)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                device.ProcessIncoming(data);
            });
        }

        public async Task<List<DeviceInfo>> GetAsync()
        {
            return await Task.FromResult(_repository.Get<Device>().Select(x => new DeviceInfo { Id = x.Id, Name = x.Name, Key = x.Key, OS = x.OS }).ToList());
        }
    }
}
