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
using System.Threading;


namespace SMClient.Services
{
    public class RabbitMQService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _meterId;
        private readonly Dictionary<string, string> _settings;
        private readonly string _token;
        public event Action<string> OnReadingPublished;
        public event Action<BillResponse> OnBillReceived;

        private const string SecretKey = "myreallysupersecretsmartmeterkey";

        public RabbitMQService(string meterId, Dictionary<string, string> settings, string token)
        {
            _meterId = meterId;
            _settings = settings;
            _token = token;

            var factory = new ConnectionFactory
            {
                HostName = _settings["HostName"],
                Port = int.Parse(_settings["Port"]),
                UserName = _settings["UserName"],
                Password = _settings["Password"],
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = _settings["HostName"],
                    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true // For demo only
                }
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            SetupQueues();
        }

        private void SetupQueues()
        {
            // Setup queues and exchanges, is grid alerts going to work this way?
            //_channel.QueueDeclare("test_queue", true, false, false, null); 
            _channel.QueueDeclare($"bills_{_meterId}", true, false, false, null);
            // Add more queue setups as needed
        }

        public string PublishReading(MeterReading reading)
        {
            try
            {
                var replyQueueName = _channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true).QueueName;

                var consumer = new EventingBasicConsumer(_channel);
                string correlationId = Guid.NewGuid().ToString();
                string response = null;

                var responseLock = new object(); 

                consumer.Received += (model, ea) =>
                {
                    // Check if the correlation ID matches
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        response = Encoding.UTF8.GetString(ea.Body.ToArray());
                        Console.WriteLine("Received response: " + response);

                        // Signal that the response is received
                        lock (responseLock)
                        {
                            Monitor.Pulse(responseLock);
                        }
                    }
                };

                // Start listening on the reply queue
                _channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

                // Serialize the message
                string message = System.Text.Json.JsonSerializer.Serialize(reading);
                var body = Encoding.UTF8.GetBytes(message);

                // Set up token
                var properties = _channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>
                {
                    { "Authorisation", _token }
                };
                properties.ReplyTo = replyQueueName; // Specify reply queue
                properties.CorrelationId = correlationId; // Set correlation ID for response tracking

                // Write the token
                Console.WriteLine("meterid: " + reading.MeterId + "and the token is " + _token);

                // Publish the message
                _channel.BasicPublish(
                    exchange: "",
                    routingKey: "test_queue", // Queue name
                    basicProperties: properties,
                    body: body
                );

                Console.WriteLine($"Published reading: {reading.Reading} kWh");
                OnReadingPublished?.Invoke($"Published reading: {reading.Reading} kWh");

                // Wait for the response
                lock (responseLock)
                {
                    if (response == null)
                    {
                        Monitor.Wait(responseLock, TimeSpan.FromSeconds(10)); // Timeout after 10 seconds
                    }
                }

                if (response == null)
                {
                    throw new TimeoutException("No response received within the timeout period.");
                }

                return response;
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
                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        // Check if the token in the headers matches the expected token
                        if (ea.BasicProperties.Headers != null &&
                            ea.BasicProperties.Headers.TryGetValue("Authorisation", out var tokenObj) &&
                            Encoding.UTF8.GetString((byte[])tokenObj) == _token)
                        {
                            // Convert received bytes to string
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            // Convert JSON string to BillResponse object
                            var billResponse = System.Text.Json.JsonSerializer
                                .Deserialize<BillResponse>(message);

                            onBillReceived(billResponse);
                            OnBillReceived?.Invoke(billResponse);
                        }
                        else
                        {
                            Console.WriteLine("Invalid token received. Ignoring message.");
                        }
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
