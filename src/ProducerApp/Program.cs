using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.Producer;
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
            var host = CreateHostBuilder(args).Build();
            var producer = host.Services.GetRequiredService<IMessageProducer>();

            var message = "Hello, World!";
            var queueName = "test-queue1";
            var exchangeName = "test-exchange";
            var routingKey = "test-key";
            var exchangeType = EasyRabbitMQ.Net.RabbitMQ.Enums.ExchangeType.Direct;

            try
            {
                producer.SendMessage<string>(message, queueName, exchangeName, routingKey, exchangeType);
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<RabbitMQSettings>(context.Configuration.GetSection("RabbitMQ"));
                    services.AddSingleton<IConnectionFactory>(sp =>
                    {
                        var rabbitMQSettings = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMQ").Get<RabbitMQSettings>();
                        return new ConnectionFactory
                        {
                            HostName = rabbitMQSettings.HostName,
                            UserName = rabbitMQSettings.UserName,
                            Password = rabbitMQSettings.Password,
                            VirtualHost = rabbitMQSettings.VirtualHost,
                            Port = rabbitMQSettings.Port,
                            Ssl = new SslOption
                            {
                                Enabled = rabbitMQSettings.Ssl.Enabled,
                                ServerName = rabbitMQSettings.Ssl.ServerName,
                                AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                    SslPolicyErrors.RemoteCertificateChainErrors
                            }
                        };
                    });
                    services.AddSingleton<IMessageProducer, MessageProducer>();
                });
    }
}
