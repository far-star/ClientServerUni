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

        public ReadingRepository(SmartMeterContext context)
        {
            _context = context;
        }
        public void AddReading(Reading reading)
        {
            _context.Readings.Add(reading);
            _context.SaveChanges();

            Console.WriteLine("\n=== Readings ===");
            foreach (var r in _context.Readings)
            {
                Console.WriteLine($"ReadingId: {r.ReadingId}, MeterId: {r.MeterId}, Timestamp: {r.Timestamp}");
                Console.WriteLine($"TotalConsumption: {r.TotalConsumption} kWh, PeakConsumption: {r.PeakConsumption} kWh, OffPeakConsumption: {r.OffPeakConsumption} kWh");
            }
        }

        public Reading GetReadingById(int readingId)
        {
            return _context.Readings.FirstOrDefault(r => r.ReadingId == readingId);
        }
    }
}
