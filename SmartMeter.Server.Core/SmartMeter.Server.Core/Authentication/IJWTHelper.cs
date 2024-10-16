using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Authentication
{
    public interface IJWTHelper
    {
        string GenerateToken(string meterId);
        bool ValidateToken(string token);
    }
}
