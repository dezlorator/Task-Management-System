using Domain.DTO;
using Domain.Models;
using Domain.Responses;

namespace BusinessLogic.Interfaces.Services
{
    public interface ITaskService
    {
        Task<CreateTaskResponce> CreateTaskAsync(CreateTaskDTO request);
        Task UpdateTaskAsync(UpdateTaskDTO request);
        Task<GetByIdResponce> GetByIdAsync(int id);
        Task<SearchTaskResponce> Search(SearchTaskDTO searchTaskDTO);
    }
}
