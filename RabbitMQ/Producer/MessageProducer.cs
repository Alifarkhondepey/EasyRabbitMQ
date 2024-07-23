using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;
using EasyRabbitMQ.Interface;
using EasyRabbitMQ.RabbitMQ.Models;

namespace EasyRabbitMQ.Producer
{
    public class MessageProducer : IMessageProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<MessageProducer> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;

        public MessageProducer(IConfiguration configuration, IConnectionFactory connectionFactory, ILogger<MessageProducer> logger)
        {
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger = logger;

            var rabbitMQSettingsSection = configuration.GetSection("RabbitMQ");
            if (rabbitMQSettingsSection == null)
            {
                throw new Exception("RabbitMQSettings section is missing in the configuration.");
            }

            _rabbitMQSettings = rabbitMQSettingsSection.Get<RabbitMQSettings>() ?? throw new Exception("Failed to bind RabbitMQSettings from configuration.");

            _logger.LogInformation("Initialized MessageProducer with RabbitMQ settings.");
        }

        public void SendMessage<T>(T message, string queueName, string exchangeName, string routingKey, EasyRabbitMQ.RabbitMQ.Enums.ExchangeType exchangeType)
        {
            try
            {
                string exchangeTypeString = exchangeType.ToString().ToLower();
                _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeTypeString);

                if (exchangeType != EasyRabbitMQ.RabbitMQ.Enums.ExchangeType.Fanout && string.IsNullOrWhiteSpace(routingKey))
                {
                    throw new ArgumentException("Routing key is required for direct and topic exchanges.");
                }


                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

                var messageString = JsonConvert.SerializeObject(message);
                var properties = _channel.CreateBasicProperties();
                properties.ContentType = "application/json";
                properties.Type = typeof(T).FullName;

                var envelope = new MessageEnvelope
                {
                    MessageType = typeof(T).FullName,
                    Body = messageString
                };

                var envelopeString = JsonConvert.SerializeObject(envelope);
                var envelopeBytes = Encoding.UTF8.GetBytes(envelopeString);

                _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: envelopeBytes);

                _logger.LogInformation($"Sent message '{messageString}' to exchange '{exchangeName}' with routing key '{routingKey}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
            }
        }

        public void CloseConnection()
        {
            _connection.Close();
            _logger.LogInformation("RabbitMQ connection closed.");
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _logger.LogInformation("Disposed RabbitMQ channel and connection.");
        }
    }

    public class RabbitMQSettings
    {
        public string? HostName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? ExchangeName { get; set; }
        public string? QueueName { get; set; }
        public string? VirtualHost { get; set; }
        public int Port { get; set; }
        public SslSettings Ssl { get; set; } = new SslSettings();
    }

    public class SslSettings
    {
        public bool Enabled { get; set; }
        public string? ServerName { get; set; }
        public string? AcceptablePolicyErrors { get; set; }
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
