using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMClient.Models
{
    // class to represent a meter reading
    public class MeterReading
    {
        public String MeterId { get; set; }
        public double Reading { get; set; }
        public DateTime Timestamp { get; set; }
        public String ReadingType { get; set; } = "CONSUMPTION"; // scalabability idea, in the future we can add more reading types
    }
}
