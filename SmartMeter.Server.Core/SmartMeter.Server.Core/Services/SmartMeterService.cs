using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Data;
using SmartMeter.Server.Core.DTOs;
using SmartMeter.Server.Core.Logging;
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
        private Logger _logger;

        public SmartMeterService(IReadingRepository readingRepository, ITokenRepository tokenRepository, 
            ITariffRepository tariffRepository, IJWTHelper jwtHelper, Logger logger)
        {
            _readingRepository = readingRepository;
            _tokenRepository = tokenRepository;
            _tariffRepository = tariffRepository;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public bool ProcessReading(string message, out MeterData? meterData)
        {            
            try
            {
                // token has already been validated so proceed with parsing
                meterData = ParseMessage(message);

                //Write message to database
                WriteMessageToDatabase(meterData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed due to {ex.Message}");
                meterData = null;
                return false;
            }
        }

        public string CalculateBill(MeterData meterData)
        {
            // Retrieve rates and consumption in a structured way
            var rates = new
            {
                OffPeak = GetCurrentTariffRates(meterData.MeterId).OffPeakRate,
                Peak = GetCurrentTariffRates(meterData.MeterId).PeakRate,
                Standard = GetCurrentTariffRates(meterData.MeterId).StandardRate
            };

            var consumption = new
            {
                OffPeak = meterData.EnergyConsumption.OffPeakConsumption,
                Peak = meterData.EnergyConsumption.PeakConsumption,
                Total = meterData.EnergyConsumption.TotalConsumption
            };

            // Calculate standard consumption
            decimal standardConsumption = consumption.Total - (consumption.OffPeak + consumption.Peak);

            // Calculate bills
            var bill = new
            {
                OffPeak = consumption.OffPeak * rates.OffPeak,
                Peak = consumption.Peak * rates.Peak,
                Standard = standardConsumption * rates.Standard
            };

            decimal totalBill = bill.OffPeak + bill.Peak + bill.Standard;

            var result = new
            {
                MeterId = meterData.MeterId,
                OffPeakBill = bill.OffPeak,
                PeakBill = bill.Peak,
                StandardBill = bill.Standard,
                TotalBill = totalBill // Include total bill here
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true // Optional, for pretty-printing JSON
            });
        }

        public Tariff GetCurrentTariffRates(string meterId)
        {
            return new Tariff
            {
                MeterId = meterId,
                StandardRate = 0.15m,
                PeakRate = 0.20m,
                OffPeakRate = 0.10m,
                StandingCharge = 0.30m,
                Timestamp = DateTime.UtcNow
            };
        }


        private void WriteMessageToDatabase(MeterData meterData)
        {
            //TODO: Why?
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

            var tariff = GetCurrentTariffRates(meterData.MeterId);

            // now done in MockTokenStorage
            /*
            var jwtToken = new JWToken
            {
                Token = meterData.Jwt,
                Timestamp = meterData.Timestamp,
                Reading = reading // Associate the JWT with the reading
            };
            */

            _readingRepository.AddReading(reading);
            //_tokenRepository.AddToken(jwtToken);
            _tariffRepository.AddTariff(tariff);

        }

        public MeterData ParseMessage(string json) => JsonSerializer.Deserialize<MeterData>(json);
    }
}
