using System;
using System.Windows.Input;

namespace Lynk.IoT.Gateway.Contracts.Models
{
    public abstract class Pin : NotifyProperyChangedBase
    {
        public Action<string> EmitStateChanged { get; set; }
        public int number { get; set; }

        public string Name { get; set; }

        public abstract void UpdateValue(object value);
    }
}
