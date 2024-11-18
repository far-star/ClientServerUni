using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
        // Declare a temporary queue for the response
        var replyQueueName = _channel.QueueDeclare().QueueName;

        // Create a consumer for the response queue
        var consumer = new EventingBasicConsumer(_channel);
        string correlationId = Guid.NewGuid().ToString();
        string response = null;

        var responseLock = new object(); // For synchronization

        consumer.Received += (model, ea) =>
        {
            // Check if the response matches the request
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                response = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine("Received response: " + response);

                // Signal that the response has been received
                lock (responseLock)
                {
                    Monitor.Pulse(responseLock);
                }
            }
        };

        // Start listening on the reply queue
        _channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: consumer);

        var message = JsonConvert.SerializeObject(reading);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.ReplyTo = replyQueueName; // Specify the reply queue
        properties.CorrelationId = correlationId; // Unique ID to correlate requests and responses


        _channel.BasicPublish(exchange: "", routingKey: "test_queue", basicProperties: properties, body: body);
        Console.WriteLine("Sent message: " + message);
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}
