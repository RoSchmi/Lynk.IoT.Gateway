using Lynk.IoT.Gateway.Contracts.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Lynk.IoT.Gateway.Contracts.ViewModels
{
    public class DeviceInfo : NotifyProperyChangedBase
    {


        private Guid _id = Guid.NewGuid();

        public event EventHandler OnConnectionClosed;

        public Guid Id
        {
            get { return _id; }
            set => SetProperty(ref _id, value);
        }

        private string _name;
        [Required]
        public string Name
        {
            get { return _name; }
            set { 
            SetProperty(ref _name, value); }
        }

        private string _os;
        public string OS
        {
            get { return _os; }
            set { SetProperty(ref _os, value); }
        }

        private string _key;

        [Required]
        public string Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }

        Socket _socket;

        [JsonIgnore]
        public Socket Socket
        {
            get => _socket;
            set
            {
                SetProperty(ref _socket, value);
            }
        }

        public ObservableCollection<Pin> Pins { get; }

        public DeviceInfo()
        {
            Pins = new ObservableCollection<Pin>();
            Pins.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    var obj = e.NewItems[0];
                    if (obj is PwmPin)
                    {
                    }
                    else if (obj is AnalogPin)
                    {
                    }
                    else if (obj is DigitalPin)
                    {
                    }

                }
            };
        }

        public IPEndPoint EndPoint
        {
            get
            {
                return ((IPEndPoint)Socket?.RemoteEndPoint);
            }
        }

        public void OnEmitStateChanged(string data)
        {
            try
            {
                if (Socket.Connected)
                    Socket.Send(System.Text.Encoding.UTF8.GetBytes($"{data}|"));
            }
            catch (Exception)
            {
                OnConnectionClosed?.Invoke(this, new EventArgs());
            }

        }

        public void ProcessIncoming(string data)
        {
            try
            {
                if (data.StartsWith("UPDATEDIGITAL="))
                {
                    data = data.Replace("UPDATEDIGITAL=", "");
                    string[] splits = data.Split(':');
                    var pinNumber = int.Parse(splits[0]);
                    var pin = this.Pins.OfType<DigitalPin>().Where(x => x.number == pinNumber).First();
                    var value = bool.Parse(splits[1]);
                    pin.UpdateValue(value);
                }
            }
            catch (Exception ex)
            {

            }
    
        }
    }
}
