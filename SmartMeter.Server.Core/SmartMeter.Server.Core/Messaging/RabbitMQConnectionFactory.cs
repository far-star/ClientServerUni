using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeter.Server.Core.Messaging
{
    public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory
    {
        public IConnection CreateConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "127.0.0.1", 
                Port = 5671, // SSL port
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = "127.0.0.1",
                    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true, // For demo purposes
                }
            };

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declare a queue to consume messages from
                    channel.QueueDeclare(queue: "test_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Received message: {0}", message);
                    };

                    // Start consuming
                    channel.BasicConsume(queue: "test_queue", autoAck: true, consumer: consumer);

                    Console.WriteLine("Waiting for messages... Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection failed: " + ex.Message);
            }

            return factory.CreateConnection();
        }     
    }
}
