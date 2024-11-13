using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.DTOs
{
    public class TariffJson
    {
        public decimal StandardRate { get; set; }
        public decimal PeakRate { get; set; }
        public decimal OffPeakRate { get; set; }
        public decimal StandingCharge { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
