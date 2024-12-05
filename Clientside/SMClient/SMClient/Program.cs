using System.Configuration;
using SMClient.Services;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Windows.Forms;

namespace SMClient
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {



            string meterId = Guid.NewGuid().ToString();
            Random random = new Random();

            RabbitMQService rabbitMQService = null;
            var displayService = new DisplayService();

            try
            {
                // Creating dictionary of RabbitMQ settings
                var rabbitMQSettings = new Dictionary<string, string>
                {
                    { "HostName", ConfigurationManager.AppSettings["RabbitMQ_HostName"] },
                    { "Port", ConfigurationManager.AppSettings["RabbitMQ_Port"] },
                    { "UserName", ConfigurationManager.AppSettings["RabbitMQ_UserName"] },
                    { "Password", ConfigurationManager.AppSettings["RabbitMQ_Password"] }
                };

                // Request token
                var tokenService = new RMQTokenService(rabbitMQSettings);
                string token = tokenService.RequestToken(meterId);
                while (token == null)
                {
                    Console.WriteLine("No Token Recieved");
                    Console.WriteLine("Waiting for token...");
                    await Task.Delay(1000);
                    token = tokenService.RequestToken(meterId);
                }
                Console.WriteLine($"Token received: {token}");
                tokenService.Dispose(); // so we don't have multiple connections per client

                rabbitMQService = new RabbitMQService(meterId, rabbitMQSettings, token);
                var meterService = new MeterReadingService(meterId);

                // Subscribe to bills
                rabbitMQService.SubscribeToBills(billResponse => {
                    Console.WriteLine($"Received bill: {billResponse}");
                    //displayService.ShowBill(billResponse);
                });

                // Start the meter monitoring loop
                while (true)
                {
                    try
                    {
                        var reading = meterService.GenerateReading();
                        rabbitMQService.PublishReading(reading);

                        // Random delay between readings (15-60 seconds)
                        await Task.Delay(random.Next(15000, 60000));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        //displayService.ShowError($"Error: {ex.Message}");
                        await Task.Delay(5000); // Wait before retry
                    }
                }
            }
            finally
            {
                rabbitMQService?.Dispose();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainScreen());

        }


    }
}