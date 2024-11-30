using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMClient.Models
{
    public class Tariff
    {
        public decimal StandardRate { get; set; }
        public decimal PeakRate { get; set; }
        public decimal OffPeakRate { get; set; }
        public decimal StandingCharge { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
