using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Data;
using SmartMeter.Server.Core.Logging;
using SmartMeter.Server.Core.Logging.Loggers;
using SmartMeter.Server.Core.Messaging;
using SmartMeter.Server.Core.Models;
using SmartMeter.Server.Core.Services;
using System;

namespace SmartMeter.Server.Core
{
    //Set up RabbitMQ Listener.
    //Add reading to the database.
    //Retrieve the reading and process it
    //Send data back to client
    internal class Program
    {
        private static JWTHelper tokenHelper = new JWTHelper();
        static void Main(string[] args)
        {
            try
            {
                // Initialize Dependency Injection container
                var services = ConfigureServices();
                var serviceProvider = services.BuildServiceProvider();

                // Start the server
                var server = serviceProvider.GetRequiredService<IServer>();
                server.Start();


                Console.ReadLine();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();
            string logDirectory = Path.Combine("..", "..", "..", "..", "Log");
            Directory.CreateDirectory(logDirectory); // Ensure the Log directory exists

            string logFilePath = Path.Combine(logDirectory, "SmartMeterActivity.txt");

            // Register logging listeners first
            services.AddSingleton<ILogListener, ConsoleLogger>();
            services.AddSingleton<ILogListener>(provider => new FileLogger(logFilePath));

            services.AddSingleton<LoggingFactory>();

            services.AddSingleton<Logger>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<LoggingFactory>();
                return loggerFactory.CreateLogger();
            });

            services.AddDbContext<SmartMeterContext>(options =>
                options.UseInMemoryDatabase("SmartMeterMockDb"));

            services.AddScoped<IReadingRepository, ReadingRepository>();
            services.AddScoped<Data.ITokenRepository, Data.TokenRepository>();
            services.AddScoped<ITariffRepository, TariffRepository>();

            services.AddScoped<IJWTHelper, JWTHelper>();
            services.AddScoped<ISmartMeterService, SmartMeterService>();
            services.AddScoped<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();

            services.AddScoped<IServer, RabbitMQServer>();

            return services;
        }
    }
}
