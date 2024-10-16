using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Models
{
    public class Reading
    {
        public int ReadingId { get; set; }
        public string MeterId { get; set; }
        public Meter Meter { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal PeakConsumption { get; set; }
        public decimal OffPeakConsumption { get; set; }
    }
}
