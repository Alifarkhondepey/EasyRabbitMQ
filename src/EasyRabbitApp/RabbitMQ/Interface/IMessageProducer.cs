

using EasyRabbitMQ.Net.RabbitMQ.Enums;

namespace EasyRabbitMQ.Net.Interface;
public interface IMessageProducer : IDisposable
{
    void SendMessage<T>(T message, string queueName, string exchangeName, string routingKey, ExchangeType exchangeType, IDictionary<string, object>? headers = null);
    void CloseConnection();
}
