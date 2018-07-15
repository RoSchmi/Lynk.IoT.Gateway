using System;
using System.ComponentModel.DataAnnotations;

namespace Lynk.IoT.Gateway.Persistent.Models
{
    public class Device : NotifyProperyChangedBase
    {
        private Guid _id = Guid.NewGuid();
        public Guid Id
        {
            get { return _id; }
            set => SetProperty(ref _id, value);
        }

        private string _name;
        [Required]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _os;
        public string OS
        {
            get { return _os; }
            set { SetProperty(ref _os, value); }
        }

        private string _key;
        [Required]
        public string Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }

    }
}
