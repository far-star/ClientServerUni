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
using Microsoft.Extensions.DependencyInjection;

namespace SmartMeter.Server.Core.Messaging
{
    public class RabbitMQServer : IServer
    {
        private readonly ISmartMeterService _smartMeterService;
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private Logger _logger;
        private readonly List<IModel> _channels = new List<IModel>();
        private readonly int _numThreads = 20;

        public RabbitMQServer(ISmartMeterService smartMeterService, IRabbitMQConnectionFactory connectionFactory, IServiceProvider serviceProvider, LoggingFactory logger)
        {
            _smartMeterService = smartMeterService;
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
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

                    Task.Run(() => StartChannelConsumer(channel, i));
                }

                _logger.Info("Connected to RabbitMQ!");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting server: {ex.Message}");
                Dispose();
            }
        }

        private void StartChannelConsumer(IModel channel, int channelNumber)
        {
            try
            {
                channel.QueueDeclare(queue: "test_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    await Task.Run(() => ProcessMessage(channel, ea));
                };

                channel.BasicConsume(queue: "test_queue", autoAck: false, consumer: consumer);
                _logger.Info($"Channel {channelNumber} is consuming messages...");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in channel {channelNumber}: {ex.Message}");
            }
        }

        private void ProcessMessage(IModel channel, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.Info($"Received message: {message}");

            using var scope = _serviceProvider.CreateScope();
            var smartMeterService = scope.ServiceProvider.GetRequiredService<ISmartMeterService>();

            if (!smartMeterService.ProcessReading(message, out var meterData))
            {
                _logger.Error("Message rejected");
                channel.BasicReject(ea.DeliveryTag, false);
                return;
            }

            try
            {
                var bill = smartMeterService.CalculateBill(meterData!);
                var responseBytes = Encoding.UTF8.GetBytes(bill);
                var responseProperties = channel.CreateBasicProperties();
                responseProperties.CorrelationId = ea.BasicProperties.CorrelationId;

                channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo,
                    basicProperties: responseProperties, body: responseBytes);
                channel.BasicAck(ea.DeliveryTag, false);

                _logger.Info($"Response sent and message acknowledged: {bill}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process message: {ex.Message}");
                channel.BasicReject(ea.DeliveryTag, false);
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
