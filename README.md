<<<<<<< HEAD
# EasyRabbitMQ.Net

This package provides a comprehensive solution for integrating RabbitMQ messaging into your .NET application. It includes classes and interfaces for message producers, consumers, factories, and hosted services for background processing.

## Features

- Message Producers
- Message Consumers
- RabbitMQ Factory
- Hosted Services for Background Processing

## Installation

1. **Add Package:**
      
	Add the package to your project using NuGet Package Manager or the .NET CLI:

       dotnet add package EasyRabbitMQ.Net

       or https://www.nuget.org/packages/EasyRabbitMQ.Net



2. **Configure RabbitMQ Settings:**

	Define your RabbitMQ settings in the appsettings.json file:

	        {
          "RabbitMQ": {
            "HostName": "your_hostname",
            "UserName": "your_username",
            "Password": "your_password",
            "VirtualHost": "/",
            "Port": 5672,
            "Ssl": {
              "Enabled": false,
              "ServerName": "",
              "AcceptablePolicyErrors": ""
            }
          }
        }

3. Register Services:
Register the necessary services in your Startup.cs or Program.cs file:

        // RabbitMQ Config
        var rabbitMQSettingsSection = configuration.GetSection("RabbitMQ");
        var rabbitMQSettings = rabbitMQSettingsSection.Get<RabbitMQSettings>();
        services.Configure<RabbitMQSettings>(rabbitMQSettingsSection);

        services.AddSingleton<IConnectionFactory>(provider =>
        {
            if (rabbitMQSettings == null)
            {
                throw new ApplicationException("RabbitMQ settings are missing or invalid.");
            }

            var factory = new ConnectionFactory
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
            Console.WriteLine(factory.ToString());

            return factory;
        });

           services.AddSingleton<RabbitMQFactory>();

        services.AddSingleton<IMessageProducer>(sp =>
        {
            var factory = sp.GetRequiredService<RabbitMQFactory>();
            return factory.CreateProducer();
        });

        // Register the IMessageConsumer service
        services.AddSingleton<IMessageConsumer>(sp =>
        {
            var factory = sp.GetRequiredService<RabbitMQFactory>();
            return factory.CreateConsumer();
        });

        services.AddHostedService<RabbitMQHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQHostedService>>();
            var messageConsumer = sp.GetRequiredService<IMessageConsumer>();
            var mediator = sp.GetRequiredService<IMediator>();
            var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var queueName = "test";
            var exchangeName = "testAppExchange";
            var routingKey = "test.add";
            var exchangeType = TotallyTech.RabbitMQ.RabbitMQ.Enums.ExchangeType.Topic;

            return new RabbitMQHostedService(logger, messageConsumer, serviceScopeFactory, queueName, exchangeName, routingKey, exchangeType, mediator);
        });

4. Implement Message Handling:
Create a hosted service to handle incoming messages and perform the necessary business logic:

        public class RabbitMQHostedService : IHostedService
        {
            private readonly ILogger<RabbitMQHostedService> _logger;
            private readonly IMessageConsumer<testDto> _messageConsumer;
            private readonly IServiceScopeFactory _serviceScopeFactory;

            public RabbitMQHostedService(ILogger<RabbitMQHostedService> logger, IMessageConsumer<testDto> messageConsumer, IServiceScopeFactory serviceScopeFactory)
            {
                _logger = logger;
                _messageConsumer = messageConsumer;
                _serviceScopeFactory = serviceScopeFactory;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("RabbitMQ Hosted Service is starting.");

                // Set up the message handler
                _messageConsumer.OnMessageReceived = MessageReceivedHandler;

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("RabbitMQ Hosted Service is stopping.");
                return Task.CompletedTask;
            }

            private void MessageReceivedHandler(testDto message)
            {
                _ = HandleMessageAsync(message);
            }

            private async Task HandleMessageAsync(testDto message)
            {
                _logger.LogInformation("MessageReceivedHandler invoked in RabbitMQHostedService.");
                _logger.LogInformation($"Received message: {message}");
                _logger.LogInformation(message.Name);

                // Create a scope to resolve scoped services
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var testService = scope.ServiceProvider.GetRequiredService<Itest>();
                    var result = await testService.Add(message, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("testDto added successfully.");
                    }
                    else
                    {
                        _logger.LogError("Failed to add testDto: {0}", result.Errors);
                    }
                }
            }
        }

5. implement send to rabbitmq method

    inject    
 
        private readonly IMessageProducer _messageProducer; // Declare MessageProducer
        public test( IMessageProducer messageProducer)
                            {
                                _messageProducer = messageProducer;
                            }

        private void SendMessageToRabbitMQ(testRequestDto command, string queueName, string exchangeName, string routingKey, ExchangeType exchangeType)
        {
            try
            {
                _messageProducer.SendMessage(command, queueName, exchangeName, routingKey, exchangeType);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine("Error sending message to RabbitMQ" + ex.Message.ToString());
            }
        }
        

## New Features

We have added two sample projects to demonstrate the usage of this package:

- **Producer Sample Project:** Demonstrates how to send messages using `EasyRabbitMQ.Net`.
- **Consumer Sample Project:** Demonstrates how to consume messages using `EasyRabbitMQ.Net` with enhanced error handling.

These projects provide a clear example of how to set up and use the `EasyRabbitMQ.Net` package for message production and consumption. You can find the complete implementation in the `ProducerApp` and `ConsumerApp` directories.

## Enhanced Error Handling

The package now includes enhanced error handling for the consumer to provide a better user experience. Detailed error information is logged, and optional requeueing can be performed for failed messages.

For more information, please visit the [GitHub repository](https://github.com/Alifarkhondepey/EasyRabbitMQ).


This `README.md` provides clear and comprehensive instructions for installing and using your `EasyRabbitMQ.Net` package.
=======
# EasyRabbitMQ

This package provides a comprehensive solution for integrating RabbitMQ messaging into your .NET application. It includes classes and interfaces for message producers, consumers, factories, and hosted services for background processing.

## Features

- Message Producers
- Message Consumers
- RabbitMQ Factory
- Hosted Services for Background Processing

## Installation

1. **Add Package:**
      
	Add the package to your project using NuGet Package Manager or the .NET CLI:

       dotnet add package EasyRabbitMQ
   or https://www.nuget.org/packages/EasyRabbitMQ.Net

3. **Configure RabbitMQ Settings:**

	Define your RabbitMQ settings in the appsettings.json file:

	        {
          "RabbitMQ": {
            "HostName": "your_hostname",
            "UserName": "your_username",
            "Password": "your_password",
            "VirtualHost": "/",
            "Port": 5672,
            "Ssl": {
              "Enabled": false,
              "ServerName": "",
              "AcceptablePolicyErrors": ""
            }
          }
        }

4. Register Services:
Register the necessary services in your Startup.cs or Program.cs file:

        // RabbitMQ Config
        var rabbitMQSettingsSection = configuration.GetSection("RabbitMQ");
        var rabbitMQSettings = rabbitMQSettingsSection.Get<RabbitMQSettings>();
        services.Configure<RabbitMQSettings>(rabbitMQSettingsSection);

        services.AddSingleton<IConnectionFactory>(provider =>
        {
            if (rabbitMQSettings == null)
            {
                throw new ApplicationException("RabbitMQ settings are missing or invalid.");
            }

            var factory = new ConnectionFactory
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
            Console.WriteLine(factory.ToString());

            return factory;
        });

           services.AddSingleton<RabbitMQFactory>();

        services.AddSingleton<IMessageProducer>(sp =>
        {
            var factory = sp.GetRequiredService<RabbitMQFactory>();
            return factory.CreateProducer();
        });

        // Register the IMessageConsumer service
        services.AddSingleton<IMessageConsumer>(sp =>
        {
            var factory = sp.GetRequiredService<RabbitMQFactory>();
            return factory.CreateConsumer();
        });

        services.AddHostedService<RabbitMQHostedService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMQHostedService>>();
            var messageConsumer = sp.GetRequiredService<IMessageConsumer>();
            var mediator = sp.GetRequiredService<IMediator>();
            var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var queueName = "test";
            var exchangeName = "testAppExchange";
            var routingKey = "test.add";
            var exchangeType = TotallyTech.RabbitMQ.RabbitMQ.Enums.ExchangeType.Topic;

            return new RabbitMQHostedService(logger, messageConsumer, serviceScopeFactory, queueName, exchangeName, routingKey, exchangeType, mediator);
        });

5. Implement Message Handling:
Create a hosted service to handle incoming messages and perform the necessary business logic:

        public class RabbitMQHostedService : IHostedService
        {
            private readonly ILogger<RabbitMQHostedService> _logger;
            private readonly IMessageConsumer<testDto> _messageConsumer;
            private readonly IServiceScopeFactory _serviceScopeFactory;

            public RabbitMQHostedService(ILogger<RabbitMQHostedService> logger, IMessageConsumer<testDto> messageConsumer, IServiceScopeFactory serviceScopeFactory)
            {
                _logger = logger;
                _messageConsumer = messageConsumer;
                _serviceScopeFactory = serviceScopeFactory;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("RabbitMQ Hosted Service is starting.");

                // Set up the message handler
                _messageConsumer.OnMessageReceived = MessageReceivedHandler;

                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("RabbitMQ Hosted Service is stopping.");
                return Task.CompletedTask;
            }

            private void MessageReceivedHandler(testDto message)
            {
                _ = HandleMessageAsync(message);
            }

            private async Task HandleMessageAsync(testDto message)
            {
                _logger.LogInformation("MessageReceivedHandler invoked in RabbitMQHostedService.");
                _logger.LogInformation($"Received message: {message}");
                _logger.LogInformation(message.Name);

                // Create a scope to resolve scoped services
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var testService = scope.ServiceProvider.GetRequiredService<Itest>();
                    var result = await testService.Add(message, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("testDto added successfully.");
                    }
                    else
                    {
                        _logger.LogError("Failed to add testDto: {0}", result.Errors);
                    }
                }
            }
        }

6. implement send to rabbitmq method

    inject    
 
        private readonly IMessageProducer _messageProducer; // Declare MessageProducer
        public test( IMessageProducer messageProducer)
                            {
                                _messageProducer = messageProducer;
                            }

        private void SendMessageToRabbitMQ(testRequestDto command, string queueName, string exchangeName, string routingKey, ExchangeType exchangeType)
        {
            try
            {
                _messageProducer.SendMessage(command, queueName, exchangeName, routingKey, exchangeType);
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine("Error sending message to RabbitMQ" + ex.Message.ToString());
            }
        }
        
This `README.md` provides clear and comprehensive instructions for installing and using your `EasyRabbitMQ` package.
>>>>>>> 43a8ebd79cafdba7c36b3fedec61d7dd6ba15a21
