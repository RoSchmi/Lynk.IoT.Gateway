using Lynk.IoT.Gateway.Contracts.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Lynk.IoT.Gateway.Views
{
    public class PinViewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PwmPinViewModelDataTemplate { get; set; }
        public DataTemplate DigitalPinViewModelDataTemplate { get; set; }
        public DataTemplate AnalogPinViewModelDataTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is PwmPin)
            {
                return PwmPinViewModelDataTemplate;
            }
            else if (item is AnalogPin)
            {
                return AnalogPinViewModelDataTemplate;
            }
            else if (item is DigitalPin)
            {
                return DigitalPinViewModelDataTemplate;
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
