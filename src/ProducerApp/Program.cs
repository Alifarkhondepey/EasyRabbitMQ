using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.Producer;
using EasyRabbitMQ.Net.RabbitMQ.Extensions;
using EasyRabbitMQ.Net.RabbitMQ.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Net.Security;

namespace ProducerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = await CreateHostBuilder(args);
            var producer = host.Services.GetRequiredService<IMessageProducer>();

            var message = "Hello, World!";
            var queueName = "test-queue1";
            var exchangeName = "test-exchange";
            var routingKey = "test-key";
            var exchangeType = EasyRabbitMQ.Net.RabbitMQ.Enums.ExchangeType.Direct;

            try
            {
               await producer.SendMessageAsync<string>(message, queueName, exchangeName, routingKey, exchangeType);
                Console.WriteLine($"Message sent with routing key: {routingKey}");
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                if (ex.ShutdownReason != null)
                {
                    Console.WriteLine($"AMQP close-reason: {ex.ShutdownReason.ReplyText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            await host.RunAsync();
        }

        public static async Task<IHost> CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add RabbitMQ services via extension method
                    services.AddRabbitMQServices(context.Configuration);

                    // Additional logging or other services can be registered here
                    services.AddLogging();
                })
                .Build();
        }





    }
}
