using SMClient.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client.Events;


namespace SMClient.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _meterId;
        private readonly IConfiguration _configuration;

        public RabbitMQService(string meterId, IConfiguration configuration)
        {
            _meterId = meterId;
            _configuration = configuration;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            SetupQueues();
        }

        private void SetupQueues()
        {
            // Setup queues and exchanges
            _channel.QueueDeclare("meter_readings", true, false, false, null);
            _channel.QueueDeclare($"bills_{_meterId}", true, false, false, null);
            // Add more queue setups as needed
        }

        public void PublishReading(MeterReading reading)
        {
            try
            {
                // Convert the reading object to JSON string
                string message = System.Text.Json.JsonSerializer.Serialize(reading);
                // Convert string to bytes for sending
                var body = Encoding.UTF8.GetBytes(message);

                // Publish to RabbitMQ
                _channel.BasicPublish(
                    exchange: "",           // Empty string means default exchange
                    routingKey: "meter_readings", // Queue name
                    basicProperties: null,   // Message properties (can set persistence, etc.)
                    body: body              // The actual message
                );

                Console.WriteLine($"Published reading: {reading.Reading} kWh");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing reading: {ex.Message}");
                throw;
            }
        }

        public void SubscribeToBills(Action<BillResponse> onBillReceived)
        {
            try
            {
                // Create a consumer that will handle received messages
                var consumer = new EventingBasicConsumer(_channel);

                // Setup what happens when a message is received
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        // Convert received bytes to string
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        // Convert JSON string to BillResponse object
                        var billResponse = System.Text.Json.JsonSerializer
                            .Deserialize<BillResponse>(message);

                        // Call the provided callback with the bill
                        onBillReceived(billResponse);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing received bill: {ex.Message}");
                    }
                };

                // Start consuming messages from the bill queue
                _channel.BasicConsume(
                    queue: $"bills_{_meterId}", // Unique queue for this meter
                    autoAck: true,              // Auto acknowledge receipt
                    consumer: consumer          // Our consumer that handles messages
                );

                Console.WriteLine("Started listening for bills");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up bill subscription: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
