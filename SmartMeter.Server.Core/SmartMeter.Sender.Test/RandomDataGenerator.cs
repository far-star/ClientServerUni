using SmartMeter.Sender.Test.Models;
using System;

public static class RandomDataGenerator
{
    private static readonly Random _random = new Random();

    public static MeterReadingMessage GenerateRandomMeterReading()
    {
        int readingId = _random.Next(10000, 99999);  // Random ID for JWT

        return new MeterReadingMessage
        {
            MeterId = $"METER-{_random.Next(1000, 9999)}",
            Timestamp = DateTime.UtcNow,
            EnergyConsumption = new EnergyConsumption
            {
                TotalConsumption = _random.Next(100, 1000),
                PeakConsumption = _random.Next(50, 500),
                OffPeakConsumption = _random.Next(50, 500)
            },
            Tariff = new TariffData
            {
                StandardRate = 0.15m,
                PeakRate = 0.20m,
                OffPeakRate = 0.10m,
                StandingCharge = 0.30m,
                Timestamp = DateTime.UtcNow
            },
            Jwt = JwtHelper.GenerateToken(readingId)  // Generate JWT referencing the reading ID
        };
    }
}
