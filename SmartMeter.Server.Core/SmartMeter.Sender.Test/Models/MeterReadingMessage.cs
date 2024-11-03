using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Sender.Test.Models
{
    public class MeterReadingMessage
    {
        public string MeterId { get; set; }
        public DateTime Timestamp { get; set; }
        public EnergyConsumption EnergyConsumption { get; set; }
        public TariffData Tariff { get; set; }
        public string Jwt { get; set; }
    }
}
