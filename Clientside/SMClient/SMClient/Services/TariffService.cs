using SMClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMClient.Services
{
    public class TariffService
    {
        // Tariffs are now generated from the server end
        // is there a use case for a smart meter deciding the tariffs itself?
        public Tariff GetCurrentTariff()
        {
            return new Tariff
            {
                StandardRate = 0.14M,
                PeakRate = 0.20M,
                OffPeakRate = 0.10M,
                StandingCharge = 0.25M,
                Timestamp = DateTime.Now
            };
        }
    }
}
