using EasyRabbitMQ.Net.Consumer;
using EasyRabbitMQ.Net.Interface;
using EasyRabbitMQ.Net.Producer;
using EasyRabbitMQ.Net.RabbitMQ.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyRabbitMQ.Net.RabbitMQ.Extensions
{
    public static class RabbitMQServiceExtensions // Ensure this class is static
    {
        public static IServiceCollection AddRabbitMQServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register RabbitMQFactory
            services.AddSingleton<RabbitMQFactory>(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return new RabbitMQFactory(configuration, loggerFactory);
            });

            // Register IMessageProducer
            services.AddSingleton<IMessageProducer>(sp =>
            {
                var factory = sp.GetRequiredService<RabbitMQFactory>();
                return factory.CreateProducer().GetAwaiter().GetResult(); // Synchronous resolution
            });

            // Register IMessageConsumer
            services.AddSingleton<IMessageConsumer>(sp =>
            {
                var factory = sp.GetRequiredService<RabbitMQFactory>();
                return factory.CreateConsumerAsync().GetAwaiter().GetResult(); // Synchronous resolution
            });

            return services;
        }
    }
}