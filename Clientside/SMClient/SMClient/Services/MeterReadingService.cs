using SMClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMClient.Services
{
    // we could do this in the program.cs or the other services but usually getting readings would be more complex
    public class MeterReadingService
    {
        private readonly Random _random = new Random();
        private double _currentReading = 1000; // Starting value
        private readonly string _meterId;

        public MeterReadingService(string meterId)
        {
            _meterId = meterId;
        }

        public MeterReading GenerateReading()
        {
            // Generate realistic reading logic
            double increase = _random.NextDouble() * 0.4 + 0.1;
            _currentReading += increase;

            return new MeterReading
            {
                MeterId = _meterId,
                Reading = Math.Round(_currentReading, 2),
                Timestamp = DateTime.Now,
                EnergyConsumption = new EnergyConsumption
                {
                    TotalConsumption = _random.Next(100, 1000),
                    PeakConsumption = _random.Next(50, 500),
                    OffPeakConsumption = _random.Next(50, 500)
                }
            };
        }
    }
}
