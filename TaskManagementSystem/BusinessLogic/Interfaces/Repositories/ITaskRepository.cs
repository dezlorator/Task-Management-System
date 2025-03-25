using Domain.DTO;
using Domain.Models;

namespace BusinessLogic.Interfaces.Repositories
{
    public interface ITaskRepository
    {
        Task<int> CreateAsync(TaskModel task);
        Task<TaskModel?> GetTaskByIdAsync(int taskId);
        Task<List<TaskModel>?> SearchAsync(SearchTaskDTO search);
        Task SaveChangesAsync();
    }
}
