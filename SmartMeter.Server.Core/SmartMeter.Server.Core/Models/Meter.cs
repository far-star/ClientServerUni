using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Models
{
    public class Meter
    {
        public string MeterId { get; set; }
        public DateTime InstallationDate { get; set; }
    }
}
