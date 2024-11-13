using RabbitMQ.Client.Events;
using RabbitMQ.Client;
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
        private IConnection _connection;
        private IModel _channel;

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
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();

                Console.WriteLine("Connected to RabbitMQ!");

                // Declare and configure the queue
                _channel.QueueDeclare(queue: "test_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                // Setup the consumer
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received message: {0}", message);

                    bool isSuccess;

                    _smartMeterService.ProcessReading(message, out isSuccess);
                    if (!isSuccess)
                    {
                        Console.WriteLine("Rejected");
                        _channel.BasicReject(ea.DeliveryTag, false);
                    };
                };

                // Start consuming messages from the queue
                _channel.BasicConsume(queue: "test_queue", autoAck: true, consumer: consumer);

                Console.WriteLine("Waiting for messages...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
                Dispose();  // Ensure cleanup on failure
            }
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            Console.WriteLine("RabbitMQ connection closed.");
        }
    }

}
