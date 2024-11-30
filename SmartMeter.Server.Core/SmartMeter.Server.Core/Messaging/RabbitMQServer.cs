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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using SmartMeter.Server.Core.Storage;
using SmartMeter.Server.Core.Models;
using System.Diagnostics.Metrics;

namespace SmartMeter.Server.Core.Messaging
{
    public class RabbitMQServer : IServer
    {
        private const string SecretKey = "myreallysupersecretsmartmeterkey";
        private readonly ISmartMeterService _smartMeterService;
        private readonly IRabbitMQConnectionFactory _connectionFactory;
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private Logger _logger;
        private readonly List<IModel> _channels = new List<IModel>();
        private readonly int _numThreads = 20;
        private readonly ITokenStorage _tokenStorage;

        public RabbitMQServer(ISmartMeterService smartMeterService, IRabbitMQConnectionFactory connectionFactory, IServiceProvider serviceProvider, LoggingFactory logger, ITokenStorage tokenStorage)
        {
            _smartMeterService = smartMeterService;
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
            _logger = logger.CreateLogger();
            _tokenStorage = tokenStorage;
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

                // Start a separate consumer for token requests
                var tokenChannel = _connection.CreateModel();
                _channels.Add(tokenChannel);
                Task.Run(() => StartTokenConsumer(tokenChannel));

                _logger.Info("Connected to RabbitMQ!");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting server: {ex.Message}");
                Dispose();
            }
        }

        private void StartTokenConsumer(IModel channel)
        {
            try
            {
                // Declare token_request queue
                channel.QueueDeclare(queue: "token_request", durable: false, exclusive: false, autoDelete: false, arguments: null);
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    ProcessTokenRequest(channel, ea);
                };

                channel.BasicConsume(queue: "token_request", autoAck: false, consumer: consumer);
                _logger.Info("Token consumer is listening...");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in token consumer: {ex.Message}");
            }
        }

        private void ProcessTokenRequest(IModel channel, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.Info($"Received token request: {message}");

            try
            {
                var request = System.Text.Json.JsonSerializer.Deserialize<TokenRequest>(message);

                // Validate the HMAC signature
                if (!ValidateClient(request.MeterId, request.Signature))
                {
                    _logger.Error($"Invalid signature for meterId: {request.MeterId}");
                    channel.BasicReject(ea.DeliveryTag, false);
                    return;
                }

                // Check if we have a token for it or generate a new one
                var existingToken = _tokenStorage.GetToken(request.MeterId);
                var token = existingToken ?? GenerateToken(request.MeterId);

                // Store the token if it's newly generated
                if (existingToken == null)
                {
                    _tokenStorage.StoreToken(request.MeterId, token);
                }

                // Prepare response
                var response = new TokenResponse { Token = token };
                var responseBody = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(response));

                var responseProperties = channel.CreateBasicProperties();
                responseProperties.CorrelationId = ea.BasicProperties.CorrelationId;

                // Publish response to ReplyTo queue
                channel.BasicPublish(exchange: "", routingKey: ea.BasicProperties.ReplyTo,
                    basicProperties: responseProperties, body: responseBody);

                channel.BasicAck(ea.DeliveryTag, false);
                _logger.Info($"Token sent for meterId: {request.MeterId} token is: {token}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process token request: {ex.Message}");
                channel.BasicReject(ea.DeliveryTag, false);
            }
        }

        private bool ValidateClient(string meterId, string signature)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(meterId));
                var computedSignature = Convert.ToBase64String(hash);
                return computedSignature == signature;
            }
        }

        private string GenerateToken(string meterId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("meterId", meterId)
            };

            var token = new JwtSecurityToken(
                issuer: "SMServer",
                audience: "SMClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

            // Validate the token from headers
            if (ea.BasicProperties.Headers == null ||
                !ea.BasicProperties.Headers.TryGetValue("Authorization", out var tokenObj) ||
                tokenObj == null ||
                !(tokenObj is byte[] tokenBytes))
            {
                _logger.Error("Missing or malformed Authorization token in message headers.");
                channel.BasicReject(ea.DeliveryTag, false);
                return;
            }

            var token = Encoding.UTF8.GetString((byte[])tokenObj);

            string? mid;
            // Extract meterId from the message
            try
            {
                var msg = System.Text.Json.JsonSerializer.Deserialize<MeterData>(message);
                mid = msg?.MeterId;
                if (mid == null)
                {
                    _logger.Error("Failed to extract meterId from the message.");
                    channel.BasicReject(ea.DeliveryTag, false);
                    return;
                }
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                _logger.Error($"JSON deserialization error: {jsonEx.Message}");
                channel.BasicReject(ea.DeliveryTag, false);
                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error: {ex.Message}");
                channel.BasicReject(ea.DeliveryTag, false);
                return;
            }

            // Validate the token for the given meterId
            if (!_tokenStorage.ValidateToken(mid, token))
            {
                _logger.Error($"Invalid token for meterId: {mid}");
                channel.BasicReject(ea.DeliveryTag, false);
                return;
            }


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
                responseProperties.Headers = new Dictionary<string, object>
                {
                    { "Authorization", token }
                };

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
            _logger.Info("RabbitMQ Token connection closed.");
        }
        private void DisposeChannel(IModel channel)
        {
            channel?.Close();
            channel?.Dispose();
        }

        private class TokenResponse
        {
            public string Token { get; set; }
        }

        private class TokenRequest
        {
            public string MeterId { get; set; }
            public string Signature { get; set; }
        }
    }
}
