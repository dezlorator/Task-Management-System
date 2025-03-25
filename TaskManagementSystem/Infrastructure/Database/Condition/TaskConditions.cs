using Domain.Enums;
using Domain.Models;

namespace Infrastructure.Database.Condition
{
    public class TaskConditions : ITaskConditions
    {
        public IQueryable<TaskModel>? ApplySearchFilter(IQueryable<TaskModel>? tasks, string searchFilter)
        {
            if (tasks == null)
                return null;

            if (!string.IsNullOrEmpty(searchFilter))
            {
                return tasks.Where(x =>
                    x.Name.Contains(searchFilter) ||
                    x.Description.Contains(searchFilter) ||
                    x.AssignedTo.Contains(searchFilter)
                );
            }

            return tasks;
        }

        public IQueryable<TaskModel>? ApplyPagination(IQueryable<TaskModel>? tasks, int startIndex, int pageSize)
        {
            if (tasks == null || pageSize <= 0)
                return null;

            startIndex = startIndex >= 0 ? startIndex : 0;

            return tasks
                .Skip(startIndex)
                .Take(pageSize);
        }

        public IQueryable<TaskModel>? ApplyNameCondition(IQueryable<TaskModel>? tasks, string? name)
        {
            if (tasks == null)
                return null;

            if (string.IsNullOrEmpty(name))
                return tasks;

            return tasks.Where(x => x.Name.Contains(name));
        }

        public IQueryable<TaskModel>? ApplyDescriptionCondition(IQueryable<TaskModel>? tasks, string? description)
        {
            if (tasks == null)
                return null;

            if (string.IsNullOrEmpty(description))
                return tasks;

            return tasks.Where(x => x.Description.Contains(description));
        }

        public IQueryable<TaskModel>? ApplyAssignedToCondition(IQueryable<TaskModel>? tasks, string? assignedTo)
        {
            if (tasks == null)
                return null;

            if (string.IsNullOrEmpty(assignedTo))
                return tasks;

            return tasks.Where(x => x.AssignedTo.Contains(assignedTo));
        }

        public IQueryable<TaskModel>? ApplyStatusCondition(IQueryable<TaskModel>? tasks, TaskModelStatus? status)
        {
            if (tasks == null)
                return null;

            if (status is null)
                return tasks;

            return tasks.Where(x => x.Status == status);
        }
    }
}
