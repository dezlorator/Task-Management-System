using Domain.Models;

namespace Infrastructure.MessageBroker
{
    public interface IMessageProduser
    {
        Task PublishTaskCreatedAsync(int taskId);
        Task PublishTaskUpdatedAsync(int taskId);
        Task PublishTaskSearchResultAsync(Guid parentId, List<TaskModel> tasks);
        Task PublishTaskCreatedResultAsync(Guid parentId, int taskId);
    }
}
