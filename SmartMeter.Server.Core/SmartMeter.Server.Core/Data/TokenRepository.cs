using SmartMeter.Server.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Data
{
    public class TokenRepository : ITokenRepository
    {
        private readonly SmartMeterContext _context;

        public TokenRepository(SmartMeterContext context)
        {
            _context = context;
        }

        public void AddToken(JWToken token)
        {
            _context.JWTokens.Add(token);
            _context.SaveChanges();
        }

        public JWToken GetTokenByReadingId(int readingId)
        {
            return _context.JWTokens.FirstOrDefault(t => t.ReadingId == readingId);
        }
    }
}
