using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        public void ProcessReading(string message, out bool isSuccess)
        {
            isSuccess = false;
            try
            {
                var jsonDocu = JsonDocument.Parse(message);
                string? token = jsonDocu.RootElement.GetProperty("Jwt").GetString();
                // token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyZWFkaW5nX2lkIjoiNzI4MTMiLCJqdGkiOiJhODMzNTcyMy1jNWRjLTRmOTMtODNhNC1kZmQ3MzBmOWFhZjgiLCJleHAiOjE3MzE0NDI2MzQsImlzcyI6Ik1ldGVyU2VuZGVyIiwiYXVkIjoiTWV0ZXJSZWNlaXZlciJ9.NqUyukK8JdrS7Tf6N2yd3O_2lg-yIV1_C2cstPzy1-k";
                // ^ test token
                if (string.IsNullOrEmpty(token) || !_jwtHelper.ValidateToken(token))
                {
                    Console.WriteLine($"Token not found or invalid: {token}");
                    return;
                }

                string? meterid = jsonDocu.RootElement.GetProperty("MeterId").GetString();
                var totalConsumption = jsonDocu.RootElement.GetProperty("EnergyConsumption").GetProperty("TotalConsumption").GetDecimal();
                isSuccess = true;
                Console.WriteLine($"Total Consumption for {meterid}: {totalConsumption} kWh");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Faile due to {ex.Message}");
            }
        }
    }
}
