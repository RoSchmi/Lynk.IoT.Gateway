using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lynk.IoT.Gateway.Contracts
{
    public abstract class NotifyProperyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void SetProperty<T>(ref T property, T value, [CallerMemberName]string propertyName = "")
        {
            if (property == null && value == null)
                return;
            else
                property = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
