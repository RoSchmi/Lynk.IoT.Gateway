using Lynk.IoT.Gateway.Contracts.Interfaces;
using Lynk.IoT.Gateway.Contracts.ViewModels;
using Prism.Commands;
using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Lynk.IoT.Gateway.ViewModels
{
    public class DevicePageViewModel : Prism.Windows.Validation.ValidatableBindableBase
    {

        string _name;
        string _key;
        string _os;

        public DelegateCommand Save { get; }

        public DelegateCommand Add { get; }

        public DelegateCommand Cancel { get; }

        public DelegateCommand Delete { get; }

        private readonly IDeviceService _deviceService;
        public ObservableCollection<DeviceInfo> Devices { get; private set; }

        private DeviceInfo _device;
        public DeviceInfo Device
        {
            get { return _device; }
            set
            {
                SetProperty(ref _device, value);
                _key = _device?.Key;
                _name = _device?.Name;
                _os = _device?.OS;
            }
        }

        public DevicePageViewModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
            LoadAsync();

            Save = new DelegateCommand(SaveDevice, () =>
            {
                return Device != null && !string.IsNullOrWhiteSpace(Device.Name) && !string.IsNullOrWhiteSpace(Device.Key);

            }).ObservesProperty(() => Device)
            .ObservesProperty(() => Device.Name)
            .ObservesProperty(() => Device.Key);

            Add = new DelegateCommand(() => { AddNewDevice(); });

            Cancel = new DelegateCommand(() => { ValidateDeviceUpdate(); }, () => Device != null).ObservesProperty(() => Device);

            Delete = new DelegateCommand(async () =>
            {
                CheckExistingConnection();
                await _deviceService.RemoveAsync(Device);
                Devices.Remove(Device);

            }, () => { return Device != null && Device.Id != Guid.Empty; })
            .ObservesProperty(() => Device)
            .ObservesProperty(() => Device.Id);

            AddNewDevice();
        }

        private void AddNewDevice()
        {
            Device = new DeviceInfo() { Id = Guid.Empty, Key = Guid.NewGuid().ToString() };
        }

        private void ValidateDeviceUpdate()
        {
            if (Device?.Id != Guid.Empty)
            {
                Device.Key = _key;
                Device.Name = _name;
                Device.OS = _os;
            }
            else AddNewDevice();

        }

        private async void SaveDevice()
        {
            CheckExistingConnection();

            if (Device.Id == Guid.Empty)
            {
                await _deviceService.AddAsync(Device);
                Devices.Add(Device);
            }
            else
            {
                await _deviceService.UpdateAsync(Device);
            }
            _key = Device?.Key;
            _name = Device?.Name;
            _os = Device.OS;
        }

        private void CheckExistingConnection()
        {
            var connected = _deviceService.Connected.Where(x => x.Id == Device.Id).FirstOrDefault();
            if (connected != null)
            {
                connected.Socket.Close(1500);
                _deviceService.Connected.Remove(connected);
            }

        }

        async void LoadAsync()
        {

            Devices = new ObservableCollection<DeviceInfo>(await _deviceService.GetAsync());
        }
    }
}
