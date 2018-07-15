using GHIElectronics.TinyCLR.BrainPad;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using Lynk.IoT.TinyCLR.ESP8266Lib;
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Lynk.IoT.TinyCLR.App
{
    public class Connection
    {
        public string SSID { get; set; }
        public string Password { get; set; }
        public string Target { get; set; }
    }
    class Program
    {

        static ArrayList _pins = new ArrayList();
        private static GpioPin _pin1;
        private static Esp8266WIFI _esp8266Client;
        private static Buzzer _buzzer;
        private static Connection _connection;

        static void Main()
        {

            _buzzer = new GHIElectronics.TinyCLR.BrainPad.Buzzer();
            

            //Setup wifi and remote computer connection
            _connection = new Connection { SSID = "MOTOROLA-3258C", Password = "0661c66a03810b23b5b1", Target = "192.168.0.6" };
            //_connection = new Connection { SSID = "K7 8181", Password = "123456789", Target = "192.168.43.133" };

            GpioController gpioController = GpioController.GetDefault();

            var display = new GHIElectronics.TinyCLR.BrainPad.Display();
            display.DrawSmallText(0, 0, "Hi there!");

            display.RefreshScreen();


            _pin1 = gpioController.OpenPin(BrainPad.Expansion.GpioPin.Int);
            _pin1.SetDriveMode(GpioPinDriveMode.Output);
            _pin1.Write(GpioPinValue.Low);


            _pins.Add(_pin1);

            InitializeButtonComponents(gpioController);
            GpioPin CH_PD = gpioController.OpenPin(BrainPad.Expansion.GpioPin.Cs);
            InitializeEsp8266Wifi(display, CH_PD);

            _buzzer.Beep();

            Thread.Sleep(-1);
        }

        static double _frequency = 100;

        private static void InitializeButtonComponents(GpioController gpioController)
        {
            var leftButton = gpioController.OpenPin(BrainPad.GpioPin.ButtonLeft);
            leftButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            leftButton.ValueChanged += (s, e) =>
            {
                _frequency = 50;
                _buzzer.StopBuzzing();
            };
            var rightButton = gpioController.OpenPin(BrainPad.GpioPin.ButtonRight);
            rightButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            rightButton.DebounceTimeout = TimeSpan.FromMilliseconds(100);
            rightButton.ValueChanged += (s, e) =>
            {
                if (_frequency == 5000)
                    return;
                _frequency += 50;
                _buzzer.StartBuzzing(_frequency);
            };

            var centerButton = gpioController.OpenPin(BrainPad.GpioPin.ButtonDown);
            centerButton.SetDriveMode(GpioPinDriveMode.InputPullUp);
            //centerButton.DebounceTimeout = TimeSpan.FromMilliseconds(100);
            centerButton.ValueChanged += (s, e) =>
            {
                if (e.Edge == GpioPinEdge.RisingEdge)
                {
                    var read = _pin1.Read() == GpioPinValue.High ? true : false;

                    _pin1.Write(read ? GpioPinValue.Low : GpioPinValue.High);

                    read = _pin1.Read() == GpioPinValue.High ? true : false;
                    if (_esp8266Client != null && _esp8266Client.ConnectionState == ConnectionState.Ready)
                        _esp8266Client?.Write($"UPDATEDIGITAL={_pin1.PinNumber}:{read.ToString()}");
                }
            };
        }

        private static void InitializeEsp8266Wifi(GHIElectronics.TinyCLR.BrainPad.Display display, GpioPin CH_PD)
        {
            _esp8266Client = new Esp8266WIFI(CH_PD, BrainPad.Expansion.UartPort.Usart1);
            _esp8266Client.OnWifiStateChanged += () =>
            {
                DisplayEspState(display, _esp8266Client);
            };
            _esp8266Client.OnIPAddressChanged += () =>
            {
                DisplayEspState(display, _esp8266Client);
                CheckAndOpenConnection(_esp8266Client);
            };
            _esp8266Client.OnDataConnectionChanged += () =>
            {
                DisplayEspState(display, _esp8266Client);
                if (_esp8266Client.ConnectionState == ConnectionState.Ready)
                {
                    SendDeviceInfo(_esp8266Client);
                }
                else if (_esp8266Client.ConnectionState == ConnectionState.Closed)
                {

                    CH_PD.Write(GpioPinValue.Low);
                    Thread.Sleep(200);
                    CH_PD.Write(GpioPinValue.High);
                    Thread.Sleep(20);

                    _esp8266Client.SetupWifiConnection(_connection.SSID, _connection.Password, WifiMode.Client);
                }

            };
            _esp8266Client.OnDataReceived += () =>
            {
                string data = _esp8266Client.Data;
                if (data.IndexOf("CMD=") >= 0)
                {
                    int idx = data.IndexOf("CMD=");
                    data = data.Substring(idx + 4);
                    if (data.ToUpper() == "GETDEVICEINFO")
                    {
                        SendDeviceInfo(_esp8266Client);
                    }
                }
                else if (data.IndexOf("SETDIGITAL=") >= 0)
                {
                    int idx = data.IndexOf("SETDIGITAL=");
                    data = data.Substring(idx + 11);
                    string[] splits = data.Split('|');
                    foreach (var split in splits)
                    {
                        if (string.IsNullOrEmpty(split))
                            continue;
                        var parts = split.Split(':');
                        SetDigitalPin(int.Parse(parts[0]), parts[1]);

                    }
                }
                else if (data.IndexOf("ERROR") >= 0 && data.IndexOf("CLOSED") >= 0)
                {
                    CheckAndOpenConnection(_esp8266Client);
                }

            };

            _esp8266Client.SetupWifiConnection(_connection.SSID, _connection.Password, WifiMode.Client);
        }

        private static void CheckAndOpenConnection(Esp8266WIFI esp8266Client)
        {
            if (esp8266Client.IPAddress != "0.0.0.0")
            {
                esp8266Client.OpenPassthroughConnection(_connection.Target, 10000);
            }
        }

        private static void SendDeviceInfo(Esp8266WIFI esp8266Client)
        {
            bool pin1State = _pin1.Read() == GpioPinValue.High ? true : false;
            string value = "{\"id\":\"E5363BAF-2C1B-4C6C-A92B-41A8DAFFF870\", " +
                            "\"key\":\"E5363BAF-2C1B-4C6C-A92B-41A8DAFFF870\", " +
                            "\"analogs\":[]," +
                            "\"digitals\":" +
                            "[" +
                                "{\"state\": " + pin1State.ToString().ToLower() + "," +
                                "\"number\":" + _pin1.PinNumber.ToString() + ", " +
                                "\"name\":\"Living Room\"}" +
                            "], \"Pwms\":[]}";
            esp8266Client.Write(value);
        }

        private static void SetDigitalPin(int pinNumber, string state)
        {
            bool isTrue = state.ToLower().Equals("true");
            for (int i = 0; i < _pins.Count; i++)
            {
                var pin = (GpioPin)_pins[i];
                if (pin.PinNumber != pinNumber)
                    continue;
                if (isTrue && pin.Read() == GpioPinValue.High)
                    continue;

                pin.Write(isTrue ? GpioPinValue.High : GpioPinValue.Low);
            }
        }


        private static void DisplayEspState(GHIElectronics.TinyCLR.BrainPad.Display display, Esp8266WIFI esp8266Client)
        {
            display.Clear();
            switch (esp8266Client.WifiState)
            {
                case WifiState.Disconnected:
                    display.DrawSmallText(0, 0, "NO WIFI");
                    break;
                case WifiState.Connected:
                case WifiState.GotIP:
                    display.DrawSmallText(0, 0, "WIFI CONNECTED");
                    break;
                case WifiState.NoAP:
                    display.DrawSmallText(0, 0, "NO AP");
                    break;
                case WifiState.Busy:
                case WifiState.Unknown:
                default:
                    display.DrawSmallText(0, 0, "ERROR");
                    break;
            }
            display.DrawSmallText(0, 10, $"IP: {esp8266Client.IPAddress}");
            string ready = esp8266Client.ConnectionState == ConnectionState.Ready ? "REMOTE CONNECTED" : "REMOTE CLOSED";
            display.DrawSmallText(0, 20, ready);
            display.RefreshScreen();
        }
    }
}
