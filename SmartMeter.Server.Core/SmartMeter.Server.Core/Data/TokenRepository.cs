using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Data
{
    public class TokenRepository : ITokenRepository
    {

        private readonly ConcurrentDictionary<string, string> _tokenStore = new();

        public void StoreToken(string meterId, string token)
        {
            _tokenStore[meterId] = token;
        }

        public string? GetToken(string meterId)
        {
            _tokenStore.TryGetValue(meterId, out var token);
            return token;
        }

        public bool ValidateToken(string meterId, string token)
        {
            return _tokenStore.TryGetValue(meterId, out var storedToken) && storedToken == token;
        }
    }
}
