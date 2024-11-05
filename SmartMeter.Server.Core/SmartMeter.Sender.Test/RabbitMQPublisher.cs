using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SmartMeter.Sender.Test.Models;

public class RabbitMQPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher()
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

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "test_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void SendMessage(MeterReadingMessage reading)
    {
        var message = JsonConvert.SerializeObject(reading);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "", routingKey: "test_queue", basicProperties: null, body: body);
        Console.WriteLine("Sent message: " + message);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}
