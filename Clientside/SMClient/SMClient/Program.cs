using Microsoft.Extensions.Configuration;
using SMClient.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static async Task Main(string[] args)
        {
            // Load configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string meterId = Guid.NewGuid().ToString();

            // Create random number generator
            Random random = new Random();

            // Setup services
            RabbitMQService rabbitMQService = null;

            try
            {
                rabbitMQService = new RabbitMQService(meterId, configuration);
                var meterService = new MeterReadingService();
                var displayService = new DisplayService();

                // Start the meter monitoring loop
                while (true)
                {
                    try
                    {
                        var reading = meterService.GenerateReading(meterId);
                        rabbitMQService.PublishReading(reading);

                        // Random delay between readings (15-60 seconds)
                        await Task.Delay(random.Next(15000, 60000));
                    }
                    catch (Exception ex)
                    {
                        //displayService.ShowError($"Error: {ex.Message}");
                        await Task.Delay(5000); // Wait before retry
                    }
                }
            }
            finally
            {
                // Cleanup
                if (rabbitMQService != null)
                {
                    rabbitMQService.Dispose();
                }
            }
        }
    }
}