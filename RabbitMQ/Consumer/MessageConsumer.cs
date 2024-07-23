using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMQ.Interface;
using EasyRabbitMQ.Producer;
using EasyRabbitMQ.RabbitMQ.Enums;
using EasyRabbitMQ.RabbitMQ.Interface;
using EasyRabbitMQ.RabbitMQ.Models;

namespace EasyRabbitMQ.Consumer
{
    public class MessageConsumer : IMessageConsumer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IConnectionFactory _connectionFactory;

        public MessageConsumer(IConfiguration configuration, IConnectionFactory connectionFactory, ILogger<MessageConsumer> logger)
        {
            _connectionFactory = connectionFactory;
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger = logger;

            var rabbitMQSettingsSection = configuration.GetSection("RabbitMQ");
            if (rabbitMQSettingsSection == null)
            {
                throw new Exception("RabbitMQSettings section is missing in the configuration.");
            }

            _rabbitMQSettings = rabbitMQSettingsSection.Get<RabbitMQSettings>() ?? throw new Exception("Failed to bind RabbitMQSettings from configuration.");

            _logger.LogInformation("Initialized MessageConsumer with RabbitMQ settings.");
        }

        public void Consume<TMessage>(string queueName, string exchangeName, string routingKey, EasyRabbitMQ.RabbitMQ.Enums.ExchangeType exchangeType, MessageReceivedCallback<TMessage> callback)
        {
            string exchangeTypeString = exchangeType.ToString().ToLower();
            _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeTypeString);

            if (exchangeType != EasyRabbitMQ.RabbitMQ.Enums.ExchangeType.Fanout && exchangeType != EasyRabbitMQ.RabbitMQ.Enums.ExchangeType.Direct && string.IsNullOrWhiteSpace(routingKey))
            {
                throw new ArgumentException("Routing key is required for direct and topic exchanges.");
            }

            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message in consumer: {messageString}");

                var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(messageString);
                _logger.LogInformation($"Received envelope: {envelope}");

                if (envelope != null)
                {
                    var messageBody = envelope.Body;
                    var deserializedMessage = JsonConvert.DeserializeObject<TMessage>(messageBody);
                    if (deserializedMessage != null)
                    {
                        callback.Invoke(deserializedMessage);
                    }
                }
                else
                {
                    _logger.LogInformation("Received an invalid message envelope.");
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Waiting for messages.");
        }

        public async Task<TMessage> ConsumeAsync<TMessage>(string queueName, string exchangeName, string routingKey, EasyRabbitMQ.RabbitMQ.Enums.ExchangeType exchangeType, CancellationToken cancellationToken = default)
        {
            var completionSource = new TaskCompletionSource<TMessage>();

            MessageReceivedCallback<TMessage> handler = null!;

            handler = message =>
            {
                completionSource.TrySetResult(message);
            };

            // Start consuming messages
            Consume(queueName, exchangeName, routingKey, exchangeType, handler);

            // Wait for a message or cancellation
            using (cancellationToken.Register(() => completionSource.TrySetCanceled()))
            {
                return await completionSource.Task;
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


}
