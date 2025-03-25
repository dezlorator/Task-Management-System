using System.Text;
using Domain.Constants;
using Infrastructure.MessageBroker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

public class MessageProduser : IMessageProduser, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);
    private readonly ILogger _logger;

    public MessageProduser(ILogger<IMessageProduser> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = Task.Run(async () => await factory.CreateConnectionAsync()).Result;
        _channel = Task.Run(async () => await _connection.CreateChannelAsync()).Result;

        _channel.ExchangeDeclareAsync(exchange: BrokerConfigurations.ExchangeNames.TaskExchange, type: ExchangeType.Topic);
        _channel.QueueDeclareAsync(BrokerConfigurations.QueueNames.TaskCreatedQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclareAsync(BrokerConfigurations.QueueNames.TaskUpdatedQueue, durable: true, exclusive: false, autoDelete: false);

        _channel.QueueBindAsync(BrokerConfigurations.QueueNames.TaskCreatedQueue, BrokerConfigurations.ExchangeNames.TaskExchange, BrokerConfigurations.QueueNames.TaskCreatedQueue);
        _channel.QueueBindAsync(BrokerConfigurations.QueueNames.TaskUpdatedQueue, BrokerConfigurations.ExchangeNames.TaskExchange, BrokerConfigurations.QueueNames.TaskUpdatedQueue);
    }

    private async Task PublishMessageAsync(string routingKey, object message)
    {
        var json = JsonConvert.SerializeObject(message);
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
                    _logger.LogError($"Max retries reached for {routingKey}. Giving up.");
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

    public void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
    }
}
