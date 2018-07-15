using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.SerialCommunication;
using GHIElectronics.TinyCLR.Storage.Streams;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Lynk.IoT.TinyCLR.ESP8266Lib
{

    public enum WifiMode { Client = 1, Server = 2 }
    public enum WifiState { Disconnected, Connected, NoAP, GotIP, Busy, Unknown }
    public enum ConnectionState
    {
        NotReady,
        Connected,
        Ready,
        Closed
    }
    public delegate void PropertChangedEventHandler();
    public class Esp8266WIFI
    {
        private static object _lock = new object();
        private readonly GpioPin _enablePin;
        private static int _lastPing = 0;
        private readonly SerialDevice _wifi;
        private readonly DataReader _dataReader;
        private readonly DataWriter _dataWriter;
        public event PropertChangedEventHandler OnWifiStateChanged;
        public event PropertChangedEventHandler OnDataConnectionChanged;
        public event PropertChangedEventHandler OnIPAddressChanged;
        public event PropertChangedEventHandler OnDataReceived;

        private string _ipAddress = "0.0.0.0";
        public string IPAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnIPAddressChanged?.Invoke();
            }
        }

        private WifiState _wifiState = WifiState.Unknown;
        public WifiState WifiState
        {
            get { return _wifiState; }
            set { _wifiState = value; OnWifiStateChanged?.Invoke(); }
        }

        private ConnectionState _dataConnectionState;

        public ConnectionState ConnectionState
        {
            get { return _dataConnectionState; }
            set
            {
                _dataConnectionState = value;
                OnDataConnectionChanged?.Invoke();
            }

        }

        private string _dataReceived;
        public string Data
        {
            get { return _dataReceived; }
            private set
            {
                _dataReceived = value;
                OnDataReceived?.Invoke();
            }
        }

        Thread _connectionWorker;

        private WifiMode _wifiMode = WifiMode.Client;

        public Esp8266WIFI(GpioPin enablePin, string portId, uint baudRate = 115200)
        {

            enablePin.SetDriveMode(GpioPinDriveMode.Output);
            _enablePin = enablePin;
            _enablePin.Write(GpioPinValue.Low);
            Thread.Sleep(200);
            _enablePin.Write(GpioPinValue.High);
            Thread.Sleep(20);
            _wifi = SerialDevice.FromId(portId);
            _wifi.BaudRate = baudRate;
            _wifi.DataBits = 8;
            _wifi.StopBits = SerialStopBitCount.One;
            _wifi.Parity = SerialParity.None;
            _wifi.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            _wifi.WriteTimeout = TimeSpan.FromMilliseconds(1000);

            _dataReader = new DataReader(_wifi.InputStream);
            _dataWriter = new DataWriter(_wifi.OutputStream);
            _connectionWorker = new Thread(ContinouslyReadWifiInput);

            _connectionWorker.Start();
            WriteCommand("AT");

        }

        public void Write(string data)
        {
            WriteCommand(data);
        }

        void ContinouslyReadWifiInput()
        {
            while (true)
            {
                try
                {
                    lock (_lock)
                    {
                        //WriteCommand("AT+CWJAP_DEF?");
                        string str = PrintCommandResponse();
                        if (str.IndexOf("WIFI CONNECTED") >= 0 || str.IndexOf("WIFI GOT IP") >= 0)
                        {
                            WifiState = WifiState.Connected;
                            if (str.IndexOf("WIFI GOT IP") >= 0)
                            {
                                WifiState = WifiState.GotIP;
                                WriteCommand("AT+CIFSR");
                                PrintCommandResponse();
                                Thread.Sleep(2000);
                                WriteCommand("AT+CIFSR");
                                str = PrintCommandResponse();
                                var splits = str.Split(new char[] { '\n' });
                                for (int i = 0; i < splits.Length; i++)
                                {
                                    if (splits[i].IndexOf("+CIFSR:STAIP") >= 0)
                                    {
                                        StringBuilder sb = new StringBuilder(splits[i]);
                                        sb = sb.Replace("\r", "").Replace("\"", "").Replace("+CIFSR:STAIP,", "");
                                        IPAddress = sb.ToString();
                                        break;
                                    }
                                }
                            }
                        }
                        else if (str.IndexOf("No AP") >= 0 || str.IndexOf("WIFI DISCONNECT") >= 0 || str.IndexOf("busy") >= 0 || str.IndexOf("FAIL") >= 0)
                        {

                            if (str.IndexOf("No AP") >= 0)
                                WifiState = WifiState.NoAP;
                            else if (str.IndexOf("WIFI DISCONNECT") >= 0)
                                WifiState = WifiState.Disconnected;
                            else if (str.IndexOf("busy") >= 0)
                                WifiState = WifiState.Busy;
                            else if (str.IndexOf("FAIL") >= 0)
                                WifiState = WifiState.Unknown;
                            IPAddress = "0.0.0.0";
                        }
                        else if (str.IndexOf("ERROR") >= 0)
                        {
                            if (str.IndexOf("ERROR") >= 0 && str.IndexOf("CLOSED") >= 0)
                            {
                                Thread.Sleep(2000);
                                ReadWriteCommand(str);
                            }
                            else
                            {
                                WifiState = WifiState.Unknown;
                            }
                        }
                        else
                        {
                            ReadWriteCommand(str);
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        private void ReadWriteCommand(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _lastPing++;
                if (_lastPing == 50)
                {
                    lock (_lock)
                    {
                        WriteCommand("+++");
                        _lastPing = 0;
                        Thread.Sleep(2000);
                        Data = PrintCommandResponse();
                        WriteCommand("AT");
                        ConnectionState = ConnectionState.Closed;
                    }

                }
                return;
            }

            Data = data;
            _lastPing = 0;
        }

        public void SetupWifiConnection(string ssid, string password, WifiMode wifiMode = WifiMode.Server)
        {
            _wifiMode = wifiMode;
            //if (_connectionWorker.IsAlive)
            //    _connectionWorker.Suspend();
            switch (wifiMode)
            {
                case WifiMode.Client:
                    WriteCommand("AT+CIPMUX=0");
                    WriteCommand("AT+CWMODE_DEF=1");
                    WriteCommand($"AT+CWJAP_DEF=\"{ssid}\",\"{password}\"");
                    break;
                case WifiMode.Server:
                    WriteCommand("AT+CIPMUX=1");
                    WriteCommand("AT+CWMODE_DEF=2");
                    WriteCommand($"AT+CWSAP_DEF=\"{ssid}\",\"{password}\",11,3");
                    break;
            }
        }

        public void Restart()
        {
            if (_connectionWorker.ThreadState == ThreadState.Running)
                _connectionWorker.Suspend();


            WriteCommand("AT");
            PrintCommandResponse();
            WriteCommand("AT+RST");
            //PrintCommandResponse();
            Thread.Sleep(5000);
            _connectionWorker.Resume();
        }

        void WriteCommand(string command)
        {
            lock (_lock)
            {
                _dataWriter.WriteString(command + Environment.NewLine);
                _dataWriter.Store();
                _dataWriter.Flush();
                Thread.Sleep(100);
            }
        }

        private string PrintCommandResponse()
        {
            try
            {
                var length = _dataReader.Load(2048);
                var read = _dataReader.ReadString(length);
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debug.WriteLine($"response - {length} | {read}");
                return read;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void OpenPassthroughConnection(string host, int port)
        {
            lock (_lock)
            {
                WriteCommand($"AT+CIPSTART=\"TCP\",\"{host}\",{port}");
                string response = PrintCommandResponse();

                if (response.IndexOf("CONNECT") >= 0)
                {
                    if (response.IndexOf("CMD=") >= 0)
                    {
                        ReadWriteCommand(response);
                    }

                    ConnectionState = ConnectionState.Connected;
                    WriteCommand("AT+CIPMODE=1");
                    response = PrintCommandResponse();
                    if (response.IndexOf("OK") >= 0)
                    {
                        WriteCommand($"AT+CIPSEND");
                        response = PrintCommandResponse();
                        if (response.IndexOf(">") >= 0)
                        {
                            ConnectionState = ConnectionState.Ready;
                        }
                    }
                    if (response.IndexOf("CMD=") >= 0)
                    {
                        ReadWriteCommand(response);
                    }
                }
                else if (response.IndexOf("ERROR") >= 0)
                {
                    Data = response;
                }
                if (response.IndexOf("CMD=") >= 0)
                {
                    ReadWriteCommand(response);
                }
            }
        }

        public void CloseConnection()
        {

        }

        public string Get(string host, string path)
        {
            lock (_lock)
            {
                WriteCommand("AT+CIPSTART=\"TCP\",\"inlynk-dev.azurewebsites.net\",80");
                string response = PrintCommandResponse();
                if (response.IndexOf("CONNECT") >= 0)
                {
                    string data = "GET /api/v1/1234/device HTTP/1.1\r\nHost:inlynk-dev.azurewebsites.net\r\ncontent-type:application/json\r\n";

                    WriteCommand($"AT+CIPSEND={data.Length}");
                    response = PrintCommandResponse();
                    if (response.IndexOf(">") >= 0)
                    {
                        WriteCommand(data);
                        response = PrintCommandResponse();
                    }
                    WriteCommand("AT+CIPCLOSE");
                }
            }


            return string.Empty;

        }
    }
}
