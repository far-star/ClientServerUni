using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Data;
using SmartMeter.Server.Core.DTOs;
using SmartMeter.Server.Core.Models;
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
        private readonly ITariffRepository _tariffRepository;
        private readonly IJWTHelper _jwtHelper;

        public SmartMeterService(IReadingRepository readingRepository, ITokenRepository tokenRepository, ITariffRepository tariffRepository, IJWTHelper jwtHelper)
        {
            _readingRepository = readingRepository;
            _tokenRepository = tokenRepository;
            _tariffRepository = tariffRepository;
            _jwtHelper = jwtHelper;
        }
        public void ProcessReading(string message, out bool isSuccess)
        {
            isSuccess = false;
            try
            {
                var meterData = ParseMessage(message);
                string? token = meterData.Jwt;
                // token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyZWFkaW5nX2lkIjoiNzI4MTMiLCJqdGkiOiJhODMzNTcyMy1jNWRjLTRmOTMtODNhNC1kZmQ3MzBmOWFhZjgiLCJleHAiOjE3MzE0NDI2MzQsImlzcyI6Ik1ldGVyU2VuZGVyIiwiYXVkIjoiTWV0ZXJSZWNlaXZlciJ9.NqUyukK8JdrS7Tf6N2yd3O_2lg-yIV1_C2cstPzy1-k";
                // ^ test token
                if (string.IsNullOrEmpty(token) || !_jwtHelper.ValidateToken(token))
                {
                    Console.WriteLine($"Token not found or invalid: {token}");
                    return;
                }

                
                //Write message to database
                WriteMessageToDatabase(meterData);
                isSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed due to {ex.Message}");
            }
        }

        private void WriteMessageToDatabase(MeterData meterData)
        {
            var meter = new Meter
            {
                MeterId = meterData.MeterId,
                InstallationDate = DateTime.UtcNow // Set a realistic installation date if not provided
            };

            var reading = new Reading
            {
                MeterId = meterData.MeterId,
                Timestamp = meterData.Timestamp,
                TotalConsumption = meterData.EnergyConsumption.TotalConsumption,
                PeakConsumption = meterData.EnergyConsumption.PeakConsumption,
                OffPeakConsumption = meterData.EnergyConsumption.OffPeakConsumption
            };

            var tariff = new Tariff
            {
                MeterId = meterData.MeterId,
                StandardRate = meterData.Tariff.StandardRate,
                PeakRate = meterData.Tariff.PeakRate,
                OffPeakRate = meterData.Tariff.OffPeakRate,
                StandingCharge = meterData.Tariff.StandingCharge,
                Timestamp = meterData.Tariff.Timestamp
            };

            var jwtToken = new JWToken
            {
                Token = meterData.Jwt,
                Timestamp = meterData.Timestamp,
                Reading = reading // Associate the JWT with the reading
            };


            _readingRepository.AddReading(reading);
            _tokenRepository.AddToken(jwtToken);
            _tariffRepository.AddTariff(tariff);

        }

        public MeterData ParseMessage(string json) => JsonSerializer.Deserialize<MeterData>(json);
    }
}
