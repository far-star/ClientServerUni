using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Models
{
    public class JWToken
    {
        public int JwtId { get; set; }
        public int ReadingId { get; set; }
        public Reading Reading { get; set; }
        public string Token { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
