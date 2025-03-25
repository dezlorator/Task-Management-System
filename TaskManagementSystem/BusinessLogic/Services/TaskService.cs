using BusinessLogic.Interfaces.Repositories;
using BusinessLogic.Interfaces.Services;
using Domain.DTO;
using Domain.Models;
using Domain.Responses;
using Infrastructure.MessageBroker;

namespace BusinessLogic.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMessageProduser _serviceBusHandler;

        public TaskService(ITaskRepository taskRepository, IMessageProduser serviceBusHandler) 
        {
            _taskRepository = taskRepository;
            _serviceBusHandler = serviceBusHandler;
        }

        public async Task<CreateTaskResponce> CreateTaskAsync(CreateTaskDTO request)
        {
            var task = new TaskModel
            {
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                AssignedTo = request.AssignedTo
            };

            var id = await _taskRepository.CreateAsync(task);
            await _serviceBusHandler.PublishTaskCreatedAsync(id);
            return new CreateTaskResponce(id);
        }

        public async Task UpdateTaskAsync(UpdateTaskDTO request)
        {
            var task = (await GetByIdAsync(request.Id))?.Task;
            var updateNeeded = false;

            if (task is null) 
            {
                throw new KeyNotFoundException("Task is not found");
            }

            if (!string.IsNullOrEmpty(request.Name) && task.Name != request.Name)
            {
                task.Name = request.Name;
                updateNeeded = true;
            }
            if (!string.IsNullOrEmpty(request.Description) && task.Description != request.Description)
            {
                task.Description = request.Description;
                updateNeeded = true;
            }
            if (request.Status is not null && task.Status != request.Status)
            {
                task.Status = request.Status.Value;
                updateNeeded = true;
            }

            if (updateNeeded)
            {
                await _serviceBusHandler.PublishTaskUpdatedAsync(request.Id);
                await _taskRepository.SaveChangesAsync();
            }
        }

        public async Task<GetByIdResponce> GetByIdAsync(int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task is null)
            {
                throw new KeyNotFoundException("Task is not found");
            }
            
            return new GetByIdResponce(task);
        }

        public async Task<SearchTaskResponce> Search(SearchTaskDTO searchTaskDTO)
        {
            var result = await _taskRepository.SearchAsync(searchTaskDTO);

            return new SearchTaskResponce(result);
        }
    }
}
