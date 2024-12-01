using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.RabbitMQ.Models;

namespace EasyRabbitMQ.Net.Producer
{
    public class MessageProducer : IMessageProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel; // Updated to IChannel
        private readonly ILogger<MessageProducer> _logger;

        public MessageProducer(IConnection connection, IChannel channel, ILogger<MessageProducer> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Asynchronous factory method for initialization
        public static async Task<MessageProducer> CreateAsync(IConfiguration configuration, ILogger<MessageProducer> logger)
        {
            var rabbitMQSettingsSection = configuration.GetSection("RabbitMQ");
            if (rabbitMQSettingsSection == null)
            {
                throw new Exception("RabbitMQSettings section is missing in the configuration.");
            }

            var rabbitMQSettings = rabbitMQSettingsSection.Get<RabbitMQSettings>() ?? throw new Exception("Failed to bind RabbitMQSettings from configuration.");
            if (rabbitMQSettings == null || rabbitMQSettings.HostName == null || rabbitMQSettings.UserName == null || rabbitMQSettings.Password == null)
            {
                throw new Exception("RabbitMQSettings section is missing in the configuration.");

            }
            var factory = new ConnectionFactory
            {
                HostName = rabbitMQSettings.HostName,
                UserName = rabbitMQSettings.UserName,
                Password = rabbitMQSettings.Password,
                Port = rabbitMQSettings.Port
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            logger.LogInformation("MessageProducer initialized.");
            return new MessageProducer(connection, channel, logger);
        }

        public async Task SendMessageAsync<T>(
            T message,
            string queueName,
            string exchangeName,
            string routingKey,
            EasyRabbitMQ.Net.RabbitMQ.Enums.ExchangeType exchangeType,
            IDictionary<string, object>? headers = null)
        {
            try
            {
                string exchangeTypeString = exchangeType.ToString().ToLower();
                await _channel.ExchangeDeclareAsync(exchange: exchangeName, type: exchangeTypeString);

                if (exchangeType != EasyRabbitMQ.Net.RabbitMQ.Enums.ExchangeType.Fanout && string.IsNullOrWhiteSpace(routingKey))
                {
                    throw new ArgumentException("Routing key is required for direct and topic exchanges.");
                }

                await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);

                var messageString = JsonConvert.SerializeObject(message);
                var properties = new BasicProperties();
                properties.ContentType = "application/json";
                properties.Type = typeof(T).FullName;

                if (headers != null && exchangeType == EasyRabbitMQ.Net.RabbitMQ.Enums.ExchangeType.Headers)
                {
                    properties.Headers = headers;
                }

                var envelope = new MessageEnvelope
                {
                    MessageType = typeof(T).FullName,
                    Body = messageString
                };

                var envelopeString = JsonConvert.SerializeObject(envelope);
                var envelopeBytes = Encoding.UTF8.GetBytes(envelopeString);

                await _channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: false, // Use false unless you require mandatory delivery
                    basicProperties: properties,
                    body: envelopeBytes,
                    cancellationToken: CancellationToken.None // Optional cancellation token
                );

                _logger.LogInformation($"Sent message '{messageString}' to exchange '{exchangeName}' with routing key '{routingKey}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
            }
        }

        public async Task CloseConnectionAsync()
        {
            await _connection.CloseAsync();
            _logger.LogInformation("RabbitMQ connection closed.");
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("Disposed RabbitMQ channel and connection.");
        }
    }

    public class MessageReceivedEventArgs<T> : EventArgs
    {
        public T Message { get; }

        public MessageReceivedEventArgs(T message)
        {
            Message = message;
        }
    }
}
