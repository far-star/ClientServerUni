using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Services
{
    public class SmartMeterService : ISmartMeterService
    {
        private readonly IReadingRepository _readingRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IJWTHelper _jwtHelper;

        public SmartMeterService(IReadingRepository readingRepository, ITokenRepository tokenRepository, IJWTHelper jwtHelper)
        {
            _readingRepository = readingRepository;
            _tokenRepository = tokenRepository;
            _jwtHelper = jwtHelper;
        }

        public void ProcessReading(string token)
        {
            if (!_jwtHelper.ValidateToken(token))
            {
                Console.WriteLine($"Invalid token: {token}");
                return;
            }

            var jwt = _tokenRepository.GetTokenByReadingId(int.Parse(token));
            if (jwt == null)
            {
                Console.WriteLine($"Token not found: {token}");
                return;
            }

            var reading = _readingRepository.GetReadingById(jwt.ReadingId);
            if (reading != null)
            {
                Console.WriteLine($"Total Consumption: {reading.TotalConsumption} kWh");
            }
        }
    }
}
