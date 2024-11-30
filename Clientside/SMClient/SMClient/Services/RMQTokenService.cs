using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;

namespace SMClient.Services
{
    public class RMQTokenService : IDisposable
    {
        private const string SecretKey = "myreallysupersecretsmartmeterkey";
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RMQTokenService(Dictionary<string, string> rabbitMQSettings)
        {
            // Initialize RabbitMQ connection and channel
            var factory = new ConnectionFactory
            {
                HostName = rabbitMQSettings["HostName"],
                Port = int.Parse(rabbitMQSettings["Port"]),
                UserName = rabbitMQSettings["UserName"],
                Password = rabbitMQSettings["Password"],
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = rabbitMQSettings["HostName"],
                    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true 
                }
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public string RequestToken(string meterId)
        {
            try
            {
                // temp queue for response
                var replyQueueName = _channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true).QueueName;

                var consumer = new EventingBasicConsumer(_channel);
                string correlationId = Guid.NewGuid().ToString();
                string token = null;

                var responseLock = new object();

                consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var responseBody = Encoding.UTF8.GetString(ea.Body.ToArray());
                        token = JsonSerializer.Deserialize<TokenResponse>(responseBody).Token;
                        Console.WriteLine("Received token response: " + token);

                        lock (responseLock)
                        {
                            Monitor.Pulse(responseLock);
                        }
                    }
                };

                _channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

                var request = new
                {
                    MeterId = meterId,
                    Signature = ComputeHmac(meterId)
                };
                var message = JsonSerializer.Serialize(request);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = _channel.CreateBasicProperties();
                properties.ReplyTo = replyQueueName;
                properties.CorrelationId = correlationId;

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: "token_request",
                    basicProperties: properties,
                    body: body
                );

                Console.WriteLine("Token request sent.");

                lock (responseLock)
                {
                    if (token == null)
                    {
                        Monitor.Wait(responseLock, TimeSpan.FromSeconds(10)); // Wait for up to 10 seconds
                    }
                }

                if (token == null)
                {
                    throw new TimeoutException("No token received within the timeout period.");
                }

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error requesting token: {ex.Message}");
                return null;
            }
        }


        private string ComputeHmac(string meterId)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(meterId));
                return Convert.ToBase64String(hash);
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            Console.WriteLine("RMQTokenService disposed");
        }

        private class TokenResponse
        {
            public string Token { get; set; }
        }
    }
}
