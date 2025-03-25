using Domain.Constants;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Domain.DTO;
using Microsoft.Extensions.DependencyInjection;
using BusinessLogic.Interfaces.Services;

namespace Infrastructure.MessageBroker
{
    public class SearchTaskMessageConsumer : BackgroundService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _maxRetries = 3;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);

        public SearchTaskMessageConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task<Task> StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: BrokerConfigurations.ExchangeNames.TaskExchange, type: ExchangeType.Topic);

            await _channel.QueueDeclareAsync(queue: BrokerConfigurations.QueueNames.SearchTaskQueue,
                                              durable: true,
                                              exclusive: false,
                                              autoDelete: false,
                                              arguments: null);

            await _channel.QueueDeclareAsync(queue: BrokerConfigurations.QueueNames.TaskSearchResponseQueue,
                                              durable: true,
                                              exclusive: false,
                                              autoDelete: false,
                                              arguments: null);

            await _channel.QueueBindAsync(BrokerConfigurations.QueueNames.SearchTaskQueue, BrokerConfigurations.ExchangeNames.TaskExchange, BrokerConfigurations.QueueNames.SearchTaskQueue);
            await _channel.QueueBindAsync(BrokerConfigurations.QueueNames.TaskSearchResponseQueue, BrokerConfigurations.ExchangeNames.TaskExchange, BrokerConfigurations.QueueNames.TaskSearchResponseQueue);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<SearchTaskDTO>(message);

                int attempts = 0;
                bool success = false;

                while (attempts < _maxRetries && !success)
                {
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                            var result = await taskService.Search(request);

                            var resultMessage = JsonSerializer.Serialize(result);
                            var resultBody = Encoding.UTF8.GetBytes(resultMessage);

                            await _channel.BasicPublishAsync(
                                exchange: BrokerConfigurations.ExchangeNames.TaskExchange,
                                routingKey: BrokerConfigurations.QueueNames.TaskSearchResponseQueue,
                                body: resultBody);

                            Console.WriteLine($"Processed and sent result: {resultMessage}");
                        }

                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        attempts++;
                        Console.WriteLine($"Error processing message: {ex.Message}");

                        if (attempts >= _maxRetries)
                        {
                            Console.WriteLine("Max retries reached. Giving up on this message.");
                            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                        }
                        else
                        {
                            Console.WriteLine($"Retrying in {_retryDelay.TotalSeconds} seconds...");
                            await Task.Delay(_retryDelay);
                        }
                    }
                }
            };

            await _channel.BasicConsumeAsync(queue: BrokerConfigurations.QueueNames.SearchTaskQueue,
                                              autoAck: false,
                                              consumer: consumer);

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            return base.StopAsync(cancellationToken);
        }
    }
}
