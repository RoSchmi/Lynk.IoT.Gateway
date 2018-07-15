using System.ComponentModel;

namespace Lynk.IoT.Gateway.Contracts.Models
{
    public class DigitalPin : Pin
    {
        private bool _state = true;
        public bool State
        {
            get { return _state; }
            set
            {
                SetProperty(ref _state, value);
                EmitStateChanged?.Invoke($"SETDIGITAL={number}:{value}");
            }
        }

        public override void UpdateValue(object value)
        {
            SetProperty(ref _state, (bool)value, "State");
        }
    }
}
