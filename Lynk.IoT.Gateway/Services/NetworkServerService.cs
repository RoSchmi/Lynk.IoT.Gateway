using Lynk.IoT.Gateway.Contracts.Interfaces;
using Lynk.IoT.Gateway.Contracts.Models;
using Lynk.IoT.Gateway.Contracts.ViewModels;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Lynk.IoT.Gateway.Services
{
    public class NetworkServerService : INetworkService
    {


        private TcpListener _listener;
        private ISettingsService _settingsService;
        private readonly IDeviceService _deviceService;
        private readonly CoreDispatcher _dispatcher;

        public event EventHandler OnStatusChanged;

        public bool ShouldRun { get; set; }

        public NetworkServerService(ISettingsService settingsService, IDeviceService deviceService, CoreDispatcher dispatcher)
        {
            _settingsService = settingsService;
            _deviceService = deviceService;
            _dispatcher = dispatcher;
        }

        public async void Start()
        {
            int port = _settingsService.GetInt("port");
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, port > 0 ? port : 10000));
            _listener.Start(5);
            ShouldRun = true;
            await Task.Factory.StartNew(async () =>
            {
                while (ShouldRun)
                {
                    //Listens for an incoming TCP connection
                    Socket connection = await _listener.AcceptSocketAsync();

                    //Start worker thread for the connection
                    await Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(2000);
                      
                            if (connection.Connected)
                            {
                                try
                                {
                                    //Get and parse the response from the GETDEVICEINFO command
                                    if (connection.Available == 0)
                                    {
                                        connection.Send(Encoding.UTF8.GetBytes("CMD=GETDEVICEINFO"));
                                        await Task.Delay(2000);
                                    }

                                    if (connection.Available == 0)
                                    {
                                        connection.Close();
                                        return;
                                    }
                                    var buffer = new byte[connection.Available];
                                    connection.Receive(buffer);
                                    string str = Encoding.UTF8.GetString(buffer);
                                    str = str.Replace("\r\n", "");
                                    Models.GetInfoModel getInfoModel = JsonConvert.DeserializeObject<Models.GetInfoModel>(str);
                                    DeviceInfo device = null;

                                    //Authenticate the incoming connection. If the device is successfully authenticated, enter loop to continually monitor connection.
                                    if ((device = await _deviceService.AuthenticateAsync(getInfoModel.Id, getInfoModel.Key)) != null)
                                    {

                                        getInfoModel.Digitals.ForEach(x => { device.Pins.Add(x); x.EmitStateChanged = device.OnEmitStateChanged; });
                                        getInfoModel.Analogs.ForEach(x => { device.Pins.Add(x); x.EmitStateChanged = device.OnEmitStateChanged; });
                                        getInfoModel.Pwms.ForEach(x => { device.Pins.Add(x); x.EmitStateChanged = device.OnEmitStateChanged; });
                                        device.OS = getInfoModel.OS;
                                        device.Socket = connection;
                                      
                                        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            _deviceService.Connected.Add(device);
                                            device.OnConnectionClosed += (s, e) =>
                                            {
                                                _deviceService.Connected.Remove((DeviceInfo)s);
                                            };
                                        });

                                        int pingDelay = 0;
                                        while (connection.Connected)
                                        {
                                            try
                                            {
                                                if (connection.Available > 0)
                                                {
                                                    var received = new byte[connection.Available];
                                                    connection.Receive(received);
                                                    string data = Encoding.UTF8.GetString(received);
                                                    string[] splits = data.Split("\r\n");
                                                    foreach (var item in splits)
                                                    {
                                                        if (string.IsNullOrWhiteSpace(item))
                                                            continue;
                                                        await _deviceService.ProcessIncomingAsync(device, item);
                                                    }

                                                }
                                                else
                                                {
                                                    pingDelay++;
                                                    if (pingDelay == 15)
                                                    {
                                                        pingDelay = 0;
                                                        connection.Send(System.Text.Encoding.UTF8.GetBytes("ping|"));
                                                    }

                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                connection.Close();
                                                await _deviceService.RemoveAsync(device);
                                            }
                                            finally
                                            {
                                            }

                                            await Task.Delay(500);
                                        }
                                    }
                                    else
                                    {
                                        connection.Send(Encoding.UTF8.GetBytes("ERROR=Access denied"));
                                        await Task.Delay(5000);
                                        connection.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    connection.Close();
                                    await Task.Delay(5000);
                                }
                            }
                            else
                            {
                                connection.Close();
                                await Task.Delay(5000);
                                ;
                            }
                        }
                        catch (Exception) { }
                    });
                }
            }, TaskCreationOptions.LongRunning);
            OnStatusChanged?.Invoke(true, new EventArgs());
        }

        public void Stop()
        {
            try
            {
                foreach (var item in _deviceService.Connected)
                {
                    item.Socket?.Close(5);
                    item.Socket.Dispose();
                    item.Socket = null;
                }
                _deviceService.Connected.Clear();
                ShouldRun = false;
                _listener?.Stop();
                _listener = null;
                OnStatusChanged?.Invoke(false, new EventArgs());
            }
            finally
            {

            }

        }
    }
}
