using EasyRabbitMQ.RabbitMQ.Enums;
using EasyRabbitMQ.RabbitMQ.Interface;

namespace EasyRabbitMQ.Interface;
public interface IMessageConsumer
{
    void Consume<TMessage>(string queueName, string exchangeName, string routingKey, ExchangeType exchangeType, MessageReceivedCallback<TMessage> callback);
    Task<TMessage> ConsumeAsync<TMessage>(string queueName, string exchangeName, string routingKey, ExchangeType exchangeType, CancellationToken cancellationToken = default);
    void CloseConnection();
}
