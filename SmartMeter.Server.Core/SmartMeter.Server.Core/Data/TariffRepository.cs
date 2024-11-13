using SmartMeter.Server.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Data
{
    public class TariffRepository : ITariffRepository
    {
        private readonly SmartMeterContext _context;

        public TariffRepository(SmartMeterContext context)
        {
            _context = context;
        }

        public void AddTariff(Tariff tariff)
        {
            _context.Tariffs.Add(tariff);
            _context.SaveChanges();

            Console.WriteLine("\n=== Tariffs ===");
            foreach (var t in _context.Tariffs)
            {
                Console.WriteLine($"TariffId: {t.TariffId}, MeterId: {t.MeterId}, Timestamp: {t.Timestamp}");
                Console.WriteLine($"StandardRate: ${t.StandardRate}, PeakRate: ${t.PeakRate}, OffPeakRate: ${t.OffPeakRate}, StandingCharge: ${t.StandingCharge}");
            }
        }
    }
}
