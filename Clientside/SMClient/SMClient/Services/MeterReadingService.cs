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

        public MeterReading GenerateReading(String meterId)
        {
            // Generate realistic reading logic
            double increase = _random.NextDouble() * 0.4 + 0.1;
            _currentReading += increase;

            return new MeterReading
            {
                MeterId = meterId,
                Reading = Math.Round(_currentReading, 2),
                Timestamp = DateTime.Now
            };
        }
    }
}
