using SmartMeter.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Messaging
{
    public class RabbitMQServer : IServer
    {
        private readonly ISmartMeterService _smartMeterService;
        private readonly IRabbitMQConnectionFactory _connectionFactory;

        public RabbitMQServer(ISmartMeterService smartMeterService, IRabbitMQConnectionFactory connectionFactory)
        {
            _smartMeterService = smartMeterService;
            _connectionFactory = connectionFactory;
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("Establishing RabbitMQ connection...");
                var connection = _connectionFactory.CreateConnection();

                Console.WriteLine("Connected to server!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            // Setup RabbitMQ consumers, queues, and listeners here
            // When a message is received, pass it to _smartMeterService for processing
        }
    }
}
