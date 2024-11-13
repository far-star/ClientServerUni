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

            Console.WriteLine("\n=== JWTokens ===");
            foreach (var jwt in _context.JWTokens)
            {
                Console.WriteLine($"JwtId: {jwt.JwtId}, ReadingId: {jwt.ReadingId}, Token: {jwt.Token}, Timestamp: {jwt.Timestamp}");
            }
        }

        public JWToken GetTokenByReadingId(int readingId)
        {
            return _context.JWTokens.FirstOrDefault(t => t.ReadingId == readingId);
        }
    }
}
