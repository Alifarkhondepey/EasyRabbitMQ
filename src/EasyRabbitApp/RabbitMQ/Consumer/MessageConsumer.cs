using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.RabbitMQ.Enums;
using EasyRabbitMQ.Net.RabbitMQ.Models;
using ExchangeType = EasyRabbitMQ.Net.RabbitMQ.Enums.ExchangeType;
using EasyRabbitMQ.Net.RabbitMQ.Interface;

namespace EasyRabbitMQ.Net.Consumer
{
    public class MessageConsumer : IMessageConsumer, IDisposable
    {
        public IConnection Connection { get; set; } // Public properties for DI initialization
        public IChannel Channel { get; set; }        // Updated to IModel for RabbitMQ compatibility
        private readonly RabbitMQSettings _rabbitMQSettings;
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IConfiguration _configuration;
        public MessageConsumer(RabbitMQSettings rabbitMQSettings, ILogger<MessageConsumer> logger, IConfiguration configuration)
        {
            _rabbitMQSettings = rabbitMQSettings ?? throw new ArgumentNullException(nameof(rabbitMQSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration;
        }
        public static async Task<MessageConsumer> CreateAsync(IConfiguration configuration, ILogger<MessageConsumer> logger)
        {
            var rabbitMQSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
            if (rabbitMQSettings == null)
            {
                throw new Exception("RabbitMQSettings section is missing or invalid.");
            }
            if (rabbitMQSettings == null || rabbitMQSettings.HostName == null || rabbitMQSettings.UserName == null || rabbitMQSettings.Password == null)
            {
                throw new Exception("RabbitMQSettings section is missing in the configuration.");

            }
            var factory = new ConnectionFactory
            {
                HostName = rabbitMQSettings.HostName,
                UserName = rabbitMQSettings.UserName,
                Password = rabbitMQSettings.Password,
                Port = rabbitMQSettings.Port,
                VirtualHost = rabbitMQSettings.VirtualHost!
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            var consumer = new MessageConsumer(rabbitMQSettings, logger,configuration)
            {
                Connection = connection,
                Channel = channel
            };

            return consumer;
        }

        public async Task EnsureChannelAsync(IConfiguration configuration, ILogger<MessageConsumer> logger)
        {
            if (Connection == null || Channel == null)
            {
                _logger.LogWarning("RabbitMQ connection or channel is not initialized. Attempting to initialize...");

                // Call CreateAsync to initialize Connection and Channel
                var initializedConsumer = await MessageConsumer.CreateAsync(configuration, logger);

                if (initializedConsumer.Connection == null || initializedConsumer.Channel == null)
                {
                    _logger.LogError("Failed to initialize RabbitMQ connection and channel.");
                    throw new InvalidOperationException("RabbitMQ connection and channel initialization failed.");
                }

                Connection = initializedConsumer.Connection;
                Channel = initializedConsumer.Channel;

                _logger.LogInformation("RabbitMQ connection and channel successfully initialized.");
            }
            else
            {
                _logger.LogDebug("RabbitMQ connection and channel are already initialized.");
            }
        }


        public async void Consume<TMessage>(string queueName, string exchangeName, string routingKey, ExchangeType exchangeType, MessageReceivedCallback<TMessage> callback)
        {
            await EnsureChannelAsync(_configuration, _logger);

            if (Channel == null)
            {
                _logger.LogError("RabbitMQ channel is null after EnsureChannelAsync.");
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");
            }

            string exchangeTypeString = exchangeType.ToString().ToLower();

            try
            {
              await  Channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: exchangeTypeString,
                    durable: false, // Match the existing 'durable' property
                    autoDelete: false,
                    arguments: null,
                    noWait: false
                    );

                if (exchangeType != ExchangeType.Fanout && string.IsNullOrWhiteSpace(routingKey))
                {
                    throw new ArgumentException("Routing key is required for direct and topic exchanges.");
                }

               await Channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
              await  Channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);

                var consumer = new AsyncEventingBasicConsumer(Channel);
                consumer.ReceivedAsync += async (model, ea) => 
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var messageString = Encoding.UTF8.GetString(body);

                        var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(messageString);
                        if (envelope != null)
                        {
                            var deserializedMessage = JsonConvert.DeserializeObject<TMessage>(envelope.Body);
                            if (deserializedMessage != null)
                            {
                                callback.Invoke(deserializedMessage);
                            }
                        }

                       await Channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing received message.");
                      await  Channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                };

              await  Channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

                _logger.LogInformation($"Consumer started for queue '{queueName}', exchange '{exchangeName}', routing key '{routingKey}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during consumer setup.");
                throw;
            }
        }

        public async Task<TMessage> ConsumeAsync<TMessage>(string queueName, string exchangeName, string routingKey, ExchangeType exchangeType, CancellationToken cancellationToken = default)
        {
            var completionSource = new TaskCompletionSource<TMessage>();

            MessageReceivedCallback<TMessage> handler = message => completionSource.TrySetResult(message);

            Consume(queueName, exchangeName, routingKey, exchangeType, handler);

            using (cancellationToken.Register(() => completionSource.TrySetCanceled()))
            {
                return await completionSource.Task;
            }
        }

        public void CloseConnection()
        {
            Channel?.Dispose();
            Connection?.Dispose();
            _logger.LogInformation("RabbitMQ connection closed.");
        }

        public void Dispose()
        {
            Channel?.Dispose();
            Connection?.Dispose();
            _logger.LogInformation("RabbitMQ resources disposed.");
        }
    }
}
