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
                    CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true // For demo purposes
                }
            };

            try
            {
                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create connection: " + ex.Message);
                throw; // Re-throw exception to handle it in the caller
            }
        }
    }

}
