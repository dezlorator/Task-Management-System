using Domain.Enums;
using Domain.Models;

namespace Infrastructure.Database.Condition
{
    public interface ITaskConditions
    {
        public IQueryable<TaskModel>? ApplySearchFilter(IQueryable<TaskModel>? tasks, string searchFilter);
        IQueryable<TaskModel>? ApplyPagination(IQueryable<TaskModel>? tasks, int startIndex, int pageSize);
        IQueryable<TaskModel>? ApplyNameCondition(IQueryable<TaskModel>? tasks, string? name);
        IQueryable<TaskModel>? ApplyDescriptionCondition(IQueryable<TaskModel>? tasks, string? description);
        IQueryable<TaskModel>? ApplyStatusCondition(IQueryable<TaskModel>? tasks, TaskModelStatus? status);
        IQueryable<TaskModel>? ApplyAssignedToCondition(IQueryable<TaskModel>? tasks, string? assignedTo);
    }
}
