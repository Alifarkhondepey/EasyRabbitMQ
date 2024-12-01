using EasyRabbitMQ.Net.Consumer;
using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.Producer;
using EasyRabbitMQ.Net.RabbitMQ.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

public class RabbitMQFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public RabbitMQFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public async Task<IMessageProducer> CreateProducer()
    {
        var logger = _loggerFactory.CreateLogger<MessageProducer>();

        var rabbitMQSettings = _configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>();
        if (rabbitMQSettings == null || string.IsNullOrWhiteSpace(rabbitMQSettings.HostName)
                 || string.IsNullOrWhiteSpace(rabbitMQSettings.UserName)
                 || string.IsNullOrWhiteSpace(rabbitMQSettings.Password)
                 || string.IsNullOrWhiteSpace(rabbitMQSettings.VirtualHost))
        {
            throw new Exception("RabbitMQSettings are missing or incomplete.");
        }

        var factory = new ConnectionFactory
        {
            HostName = rabbitMQSettings.HostName,
            UserName = rabbitMQSettings.UserName,
            Password = rabbitMQSettings.Password,
            Port = rabbitMQSettings.Port,
            VirtualHost = rabbitMQSettings.VirtualHost
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return new MessageProducer(connection, channel, logger);
    }

    public async Task<IMessageConsumer> CreateConsumerAsync()
    {
        var logger = _loggerFactory.CreateLogger<MessageConsumer>();
        return await MessageConsumer.CreateAsync(_configuration, logger);
    }
}
