using BusinessLogic.Interfaces.Repositories;
using Domain.DTO;
using Domain.Models;
using Infrastructure.Database.Condition;
using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskConditions _taskConditions;
        public TaskRepository(ApplicationDbContext context, ITaskConditions taskConditions) 
        {
            _context = context;
            _taskConditions = taskConditions;
        }

        public async Task<int> CreateAsync(TaskModel task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return task.Id;
        }

        public async Task<TaskModel?> GetTaskByIdAsync(int taskId)
        {
            return await _context.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);
        }

        public async Task<List<TaskModel>?> SearchAsync(SearchTaskDTO search)
        {
            var tasks = _context.Tasks.AsQueryable();

            tasks = _taskConditions.ApplyNameCondition(tasks, search.Name);
            tasks = _taskConditions.ApplyDescriptionCondition(tasks, search.Description);
            tasks = _taskConditions.ApplyStatusCondition(tasks, search.Status);
            tasks = _taskConditions.ApplyAssignedToCondition(tasks, search.AssignedTo);
            tasks = _taskConditions.ApplySearchFilter(tasks, search.SearchFilter);
            tasks = _taskConditions.ApplyPagination(tasks, search.PageNumber, search.PageSize);

            return await tasks?.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
