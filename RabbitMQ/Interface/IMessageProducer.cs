

using EasyRabbitMQ.RabbitMQ.Enums;

namespace EasyRabbitMQ.Interface;
public interface IMessageProducer : IDisposable
{
    void SendMessage<T>(T message, string queueName, string exchangeName, string routingKey, ExchangeType exchangeType);
    void CloseConnection();
}
