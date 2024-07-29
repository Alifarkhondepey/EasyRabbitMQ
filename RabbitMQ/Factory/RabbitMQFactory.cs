using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using EasyRabbitMQ.Net.Consumer;
using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.Producer;

namespace EasyRabbitMQ.Net.RabbitMQ.Factory
{
    public class RabbitMQFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILoggerFactory _loggerFactory;

        public RabbitMQFactory(IConfiguration configuration, IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _connectionFactory = connectionFactory;
            _loggerFactory = loggerFactory;
        }

        public IMessageProducer CreateProducer()
        {
            var logger = _loggerFactory.CreateLogger<MessageProducer>();
            return new MessageProducer(_configuration, _connectionFactory, logger);
        }

        public IMessageConsumer CreateConsumer()
        {
            var logger = _loggerFactory.CreateLogger<MessageConsumer>();
            return new MessageConsumer(_configuration, _connectionFactory, logger);
        }
    }
}
