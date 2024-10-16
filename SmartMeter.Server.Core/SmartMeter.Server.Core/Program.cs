using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Data;
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
            // Initialize Dependency Injection container
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Start the server
            var server = serviceProvider.GetRequiredService<IServer>();
            server.Start();


            Console.ReadLine();
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            // Add DbContext
            services.AddDbContext<SmartMeterContext>(options =>
                options.UseInMemoryDatabase("SmartMeterMockDb"));

            // Add repositories
            services.AddScoped<IReadingRepository, ReadingRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();

            // Add services
            services.AddScoped<IJWTHelper, JWTHelper>();
            services.AddScoped<ISmartMeterService, SmartMeterService>();
            services.AddScoped<IRabbitMQConnectionFactory, RabbitMQConnectionFactory>();

            // Add server
            services.AddScoped<IServer, RabbitMQServer>();

            return services;
        }
    }
}
