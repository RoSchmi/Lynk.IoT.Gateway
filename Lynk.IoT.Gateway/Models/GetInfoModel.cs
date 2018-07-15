using Lynk.IoT.Gateway.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lynk.IoT.Gateway.Models
{
    public class GetInfoModel
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public List<AnalogPin> Analogs { get; set; } = new List<AnalogPin>();
        public List<DigitalPin> Digitals { get; set; } = new List<DigitalPin>();
        public List<PwmPin> Pwms { get; set; } = new List<PwmPin>();
        public string OS { get; set; }
    }
}
