using System;
using System.ComponentModel.DataAnnotations;

namespace Lynk.IoT.Gateway.Persistent.Models
{
    public class Setting : NotifyProperyChangedBase
    {

        private Guid _id = Guid.NewGuid();
        public Guid Id
        {
            get { return _id; }
            set => SetProperty(ref _id, value);
        }

        private string _key;
        [Required]
        public string Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }

        private string _value;
        [Required]
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

    }
}
