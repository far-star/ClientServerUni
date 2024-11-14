using SmartMeter.Server.Core.Logging;
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
        private Logger _logger;

        public TariffRepository(SmartMeterContext context, Logger logger)
        {
            _context = context;
            _logger = logger;
        }

        public void AddTariff(Tariff tariff)
        {
            _context.Tariffs.Add(tariff);
            _context.SaveChanges();

            _logger.Info("\n=== Tariffs ===");
            foreach (var t in _context.Tariffs)
            {
                _logger.Info($"TariffId: {t.TariffId}, MeterId: {t.MeterId}, Timestamp: {t.Timestamp}");
                _logger.Info($"StandardRate: ${t.StandardRate}, PeakRate: ${t.PeakRate}, OffPeakRate: ${t.OffPeakRate}, StandingCharge: ${t.StandingCharge}");
            }
        }
    }
}
