using BusinessLogic.Services.ConsumerHandlers;
using Domain.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
public class MessageConsumer : BackgroundService
{
    private IChannel? _channel;
    private IConnection _connection;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);
    private readonly Dictionary<string, IConsumerHandler> _handlers;
    private readonly ILogger<MessageConsumer> _logger;

    public MessageConsumer(IConnection connection, Dictionary<string, IConsumerHandler> handlers, ILogger<MessageConsumer> logger)
    {
        _connection = connection;
        _handlers = handlers;
        _logger = logger;
    }

    public override async Task<Task> StartAsync(CancellationToken cancellationToken)
    {
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(exchange: BrokerConfigurations.ExchangeNames.TaskExchange, type: ExchangeType.Topic);

        var queues = new[] {
            BrokerConfigurations.QueueNames.CreateTaskQueue,
            BrokerConfigurations.QueueNames.SearchTaskQueue
        };

        foreach (var queue in queues)
        {
            await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueBindAsync(queue, BrokerConfigurations.ExchangeNames.TaskExchange, queue);
        }

        return base.StartAsync(cancellationToken);
    }

    protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) => await ProcessMessage(ea);

        await _channel.BasicConsumeAsync(queue: BrokerConfigurations.QueueNames.CreateTaskQueue, autoAck: false, consumer: consumer);
        await _channel.BasicConsumeAsync(queue: BrokerConfigurations.QueueNames.SearchTaskQueue, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessage(BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        int attempts = 0;
        bool success = false;

        while (attempts < _maxRetries && !success)
        {
            try
            {
                if (_handlers.TryGetValue(ea.RoutingKey, out var handler))
                {
                    await handler.Handle(message);
                }
                else
                {
                    throw new KeyNotFoundException($"No handler found for routing key {ea.RoutingKey}");
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                success = true;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            catch (Exception ex)
            {
                attempts++;
                _logger.LogError($"Error processing message: {ex.Message}");

                if (attempts >= _maxRetries)
                {
                    _logger.LogError("Max retries reached. Giving up on this message.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
                else
                {
                    _logger.LogWarning($"Retrying in {_retryDelay.TotalSeconds} seconds...");
                    await Task.Delay(_retryDelay);
                }
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.CloseAsync();
        return base.StopAsync(cancellationToken);
    }
}
