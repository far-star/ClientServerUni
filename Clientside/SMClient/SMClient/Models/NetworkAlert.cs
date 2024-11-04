using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMClient.Models
{
    public class NetworkAlert
    {
        public string ErrorType { get; set; } // network failure, server failure, authentication failure etc
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
