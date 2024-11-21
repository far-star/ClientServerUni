using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using SmartMeter.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartMeter.Server.Core.Logging;
using SmartMeter.Server.Core.Logging.Loggers;
using SmartMeter.Server.Core.DTOs;

namespace SmartMeter.Server.Core.Messaging
{
    public class RabbitMQServer : IServer
    {
        private readonly ISmartMeterService _smartMeterService;
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private IConnection _connection;
        private Logger _logger;
        private readonly List<IModel> _channels = new List<IModel>();
        private readonly int _numThreads = 20;

        public RabbitMQServer(ISmartMeterService smartMeterService, IRabbitMQConnectionFactory connectionFactory, LoggingFactory logger)
        {
            _smartMeterService = smartMeterService;
            _connectionFactory = connectionFactory;
            _logger = logger.CreateLogger();
        }

        public void Start()
        {
            try
            {
                _logger.Info("Establishing RabbitMQ connection...");
                _connection = _connectionFactory.CreateConnection();

                for (int i = 0; i < _numThreads; i++)
                {
                    var channel = _connection.CreateModel();
                    _channels.Add(channel);

                    Thread channelThread = new Thread(() => StartChannelConsumer(channel, i));
                    channelThread.Start();
                }

                _logger.Info("Connected to RabbitMQ!");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting server: {ex.Message}");
                Dispose();
            }
        }

        public void StartChannelConsumer(IModel channel, int channelNumber)
        {
            try
            {
                // Declare and configure the queue
                channel.QueueDeclare(queue: "test_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                // Setup the consumer
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.Info($"Received message: {message}");

                    var replyTo = ea.BasicProperties.ReplyTo;
                    var correlationId = ea.BasicProperties.CorrelationId;

                    if (string.IsNullOrEmpty(replyTo))
                    {
                        _logger.Error("Message does not have a reply-to property. Skipping...");
                        channel.BasicReject(ea.DeliveryTag, false);
                        return;
                    }

                    
                    MeterData? meterData;

                    bool isSuccess = _smartMeterService.ProcessReading(message, out meterData);
                    if (!isSuccess || meterData == null )
                    {
                        _logger.Error("Message Rejected");
                        channel.BasicReject(ea.DeliveryTag, false);
                        return;
                    }

                    var bill = _smartMeterService.CalculateBill(meterData);

                    try
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(bill);
                        var responseProperties = channel.CreateBasicProperties();
                        responseProperties.CorrelationId = correlationId; // Preserve correlation ID

                        channel.BasicPublish(exchange: "", routingKey: replyTo, basicProperties: responseProperties, body: responseBytes);
                        _logger.Info($"Response sent to {replyTo}: {bill}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to send response: {ex.Message}");
                    }

                    _logger.Success($"Message Acknowledged");
                    channel.BasicAck(ea.DeliveryTag, false);

                };

                // Start consuming messages from the queue
                channel.BasicConsume(queue: "test_queue", autoAck: false, consumer: consumer);

                _logger.Info("Waiting for messages...");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting server: {ex.Message}");
                Dispose();  // Ensure cleanup on failure
            }
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            foreach (var channel in _channels)
            {
                DisposeChannel(channel);
            }
            _connection?.Close();
            _connection?.Dispose();
            _logger.Info("RabbitMQ connection closed.");
        }
        private void DisposeChannel(IModel channel)
        {
            channel?.Close();
            channel?.Dispose();
        }
    }
}
