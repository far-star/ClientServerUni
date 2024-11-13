using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Services
{
    public interface ISmartMeterService
    {
        void ProcessReading(string message, out bool isSuccess);
    }
}
