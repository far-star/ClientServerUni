using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMClient.Models
{
    // class to represent the servers bill response
    public class BillResponse
    {
        public String MeterId { get; set; }
        public double Amount { get; set; }
        public double UnitPrice { get; set; }
        public DateTime BillDate { get; set; }
    }
}
