namespace Lynk.IoT.Gateway.Contracts.Models
{
    public sealed class PwmPin : Pin
    {
        private double _frequency;
        public double Frequency
        {
            get { return _frequency; }
            set { SetProperty(ref _frequency, value); }
        }

        private double _dutyCycle;
        public double DutyCycle
        {
            get { return _dutyCycle; }
            set { SetProperty(ref _dutyCycle, value); }
        }

        public override void UpdateValue(object value)
        {
            throw new System.NotImplementedException();
        }
    }
}
