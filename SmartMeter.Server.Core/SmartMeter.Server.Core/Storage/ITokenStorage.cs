using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Storage
{
    public interface ITokenStorage
    {
        void StoreToken(string meterId, string token);
        string? GetToken(string meterId);
        bool ValidateToken(string meterId, string token);
    }
}
