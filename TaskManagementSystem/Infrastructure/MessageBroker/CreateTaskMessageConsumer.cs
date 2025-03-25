using BusinessLogic.Interfaces.Services;
using Domain.Constants;
using Domain.DTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System;
using System.Threading.Tasks;

public class CreateTaskMessageConsumer : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);

    public CreateTaskMessageConsumer(IServiceProvider serviceProvider)
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

        await _channel.QueueDeclareAsync(queue: BrokerConfigurations.QueueNames.CreateTaskQueue,
                                          durable: true,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);

        await _channel.QueueBindAsync(BrokerConfigurations.QueueNames.CreateTaskQueue,
                                      BrokerConfigurations.ExchangeNames.TaskExchange,
                                      BrokerConfigurations.QueueNames.CreateTaskQueue);

        return base.StartAsync(cancellationToken);
    }

    protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var request = JsonSerializer.Deserialize<CreateTaskDTO>(message);

            int attempts = 0;
            bool success = false;

            while (attempts < _maxRetries && !success)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                        await taskService.CreateTaskAsync(request);
                    }

                    Console.WriteLine($"Received message: {message}");
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

        await _channel.BasicConsumeAsync(queue: BrokerConfigurations.QueueNames.CreateTaskQueue,
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
