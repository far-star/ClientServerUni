using SmartMeter.Server.Core.Logging;
using SmartMeter.Server.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Data
{
    public class ReadingRepository : IReadingRepository
    {
        private readonly SmartMeterContext _context;
        private Logger _logger;

        public ReadingRepository(SmartMeterContext context, Logger logger)
        {
            _context = context;
            _logger = logger;
        }
        public void AddReading(Reading reading)
        {
            _context.Readings.Add(reading);
            _context.SaveChanges();

            _logger.Info("\n=== Readings ===");
            foreach (var r in _context.Readings)
            {
                _logger.Info($"ReadingId: {r.ReadingId}, MeterId: {r.MeterId}, Timestamp: {r.Timestamp}");
                _logger.Info($"TotalConsumption: {r.TotalConsumption} kWh, PeakConsumption: {r.PeakConsumption} kWh, OffPeakConsumption: {r.OffPeakConsumption} kWh");
            }
        }

        public Reading GetReadingById(int readingId)
        {
            return _context.Readings.FirstOrDefault(r => r.ReadingId == readingId);
        }
    }
}
