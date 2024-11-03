using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Sender.Test.Models
{
    public class EnergyConsumption
    {
        public decimal TotalConsumption { get; set; }
        public decimal PeakConsumption { get; set; }
        public decimal OffPeakConsumption { get; set; }
    }
}
