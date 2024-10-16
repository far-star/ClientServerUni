using RabbitMQ.Client;
using System.Text;

namespace SmartMeter.Sender.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "127.0.0.1", // Use the correct hostname
                Port = 5671, // SSL port
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = "localhost", // Adjust to your certificate CN
                    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true // For testing only
                }
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare a queue named "test_queue"
                channel.QueueDeclare(queue: "test_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // Create a message to send
                string message = "Hello RabbitMQ!";
                var body = Encoding.UTF8.GetBytes(message);

                // Publish the message to the queue
                channel.BasicPublish(exchange: "",
                                     routingKey: "test_queue",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
