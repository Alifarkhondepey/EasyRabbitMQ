using EasyRabbitMQ.Net.Consumer;
using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.RabbitMQ.Extensions;
using EasyRabbitMQ.Net.RabbitMQ.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ConsumerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build(); // Build the host

            // Resolve the IMessageConsumer from the DI container
            var consumer = host.Services.GetRequiredService<IMessageConsumer>();

            // Define RabbitMQ settings
            var queueName = "test-queue1";
            var exchangeName = "test-exchange";
            var routingKey = "test-key";
            var exchangeType = ExchangeType.Direct;

            try
            {
                // Start consuming messages
                consumer.Consume<string>(queueName, exchangeName, routingKey, exchangeType, message =>
                {
                    Console.WriteLine($"Received message: {message}");
                });
                Console.WriteLine($"Consumer listening with routing key: {routingKey}");
            }
            catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
            {
                Console.WriteLine($"Error consuming message: {ex.Message}");
                if (ex.ShutdownReason != null)
                {
                    Console.WriteLine($"AMQP close-reason: {ex.ShutdownReason.ReplyText}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            Console.WriteLine("Waiting for messages...");
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Load RabbitMQ configuration from appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Add RabbitMQ services using the extension method
                    services.AddRabbitMQServices(context.Configuration);

                    // Register additional logging if needed
                    services.AddLogging();
                });
        }
    }
}
