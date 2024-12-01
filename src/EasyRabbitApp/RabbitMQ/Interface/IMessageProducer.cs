using EasyRabbitMQ.Net.RabbitMQ.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyRabbitMQ.Net.Interface
{
    public interface IMessageProducer : IDisposable
    {
        /// <summary>
        /// Sends a message to the specified RabbitMQ queue asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the message to send.</typeparam>
        /// <param name="message">The message to send.</param>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="exchangeName">The name of the exchange.</param>
        /// <param name="routingKey">The routing key.</param>
        /// <param name="exchangeType">The type of the exchange.</param>
        /// <param name="headers">Optional headers for the message.</param>
        Task SendMessageAsync<T>(
            T message,
            string queueName,
            string exchangeName,
            string routingKey,
            ExchangeType exchangeType,
            IDictionary<string, object>? headers = null
        );

        /// <summary>
        /// Closes the RabbitMQ connection asynchronously.
        /// </summary>
        Task CloseConnectionAsync();
    }
}
