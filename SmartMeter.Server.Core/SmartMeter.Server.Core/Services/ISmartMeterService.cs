using SmartMeter.Server.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Services
{
    public interface ISmartMeterService
    {
        bool ProcessReading(string message, out MeterData? meterData);
        string CalculateBill(MeterData meterData);
    }
}
