using System.Text;
using Domain.Constants;
using Domain.Models;
using Infrastructure.MessageBroker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

public class MessageProduser : IMessageProduser, IDisposable
{
    private readonly IChannel _channel;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);
    private readonly ILogger _logger;

    private MessageProduser(ILogger<IMessageProduser> logger, IChannel channel)
    {
        _logger = logger;
        _channel = channel;
    }

    public static async Task<MessageProduser> Create(ILogger<IMessageProduser> logger, IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: BrokerConfigurations.ExchangeNames.TaskExchange,
            type: ExchangeType.Topic
        );

        var queues = new[]
        {
        BrokerConfigurations.QueueNames.TaskCreatedQueue,
        BrokerConfigurations.QueueNames.TaskUpdatedQueue,
        BrokerConfigurations.QueueNames.TaskSearchResponseQueue,
        BrokerConfigurations.QueueNames.CreateTaskResponceQueue
    };

        foreach (var queue in queues)
        {
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queue, BrokerConfigurations.ExchangeNames.TaskExchange, queue);
        }

        return new MessageProduser(logger, channel);
    }

    private async Task PublishMessageAsync(string routingKey, object message)
    {
        var bodyModel = new { Date = DateTime.UtcNow, Guid = Guid.NewGuid(), Payload = message };
        var json = JsonConvert.SerializeObject(bodyModel);
        var body = Encoding.UTF8.GetBytes(json);

        int attempts = 0;
        bool success = false;

        while (attempts < _maxRetries && !success)
        {
            try
            {
                await _channel.BasicPublishAsync(
                    exchange: BrokerConfigurations.ExchangeNames.TaskExchange,
                    routingKey: routingKey,
                    body: body
                );

                success = true;
            }
            catch (Exception ex)
            {
                attempts++;
                _logger.LogError($"Error publishing message to {routingKey}: {ex.Message}");

                if (attempts >= _maxRetries)
                {
                    _logger.LogWarning($"Max retries reached for {routingKey}. Giving up.");
                }
                else
                {
                    _logger.LogError($"Retrying in {_retryDelay.TotalSeconds} seconds...");
                    await Task.Delay(_retryDelay);
                }
            }
        }
    }

    public async Task PublishTaskCreatedAsync(int taskId)
    {
        await PublishMessageAsync(BrokerConfigurations.QueueNames.TaskCreatedQueue, new { Id = taskId });
    }

    public async Task PublishTaskUpdatedAsync(int taskId)
    {
        await PublishMessageAsync(BrokerConfigurations.QueueNames.TaskUpdatedQueue, new { Id = taskId });
    }

    public async Task PublishTaskSearchResultAsync(Guid parentId, List<TaskModel> tasks)
    {
        await PublishMessageAsync(BrokerConfigurations.QueueNames.TaskSearchResponseQueue, new { ParentId = parentId, Responce = tasks });
    }

    public async Task PublishTaskCreatedResultAsync(Guid parentId, int taskId)
    {
        await PublishMessageAsync(BrokerConfigurations.QueueNames.CreateTaskResponceQueue, new { ParentId = parentId, TaskId = taskId });
    }

    public void Dispose()
    {
        _channel?.CloseAsync();
    }
}
