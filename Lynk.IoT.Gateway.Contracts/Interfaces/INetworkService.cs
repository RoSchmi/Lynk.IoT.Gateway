using System;
using System.Collections.Generic;
using System.Text;

namespace Lynk.IoT.Gateway.Contracts.Interfaces
{
    public interface INetworkService
    {
        event EventHandler OnStatusChanged;
        void Start();
        void Stop();
    }
}
