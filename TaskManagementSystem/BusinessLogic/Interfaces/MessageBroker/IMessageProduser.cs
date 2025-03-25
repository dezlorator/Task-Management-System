namespace Infrastructure.MessageBroker
{
    public interface IMessageProduser
    {
        Task PublishTaskCreatedAsync(int taskId);
        Task PublishTaskUpdatedAsync(int taskId);
    }
}
