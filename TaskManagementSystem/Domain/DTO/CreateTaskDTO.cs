using Domain.Enums;

namespace Domain.DTO
{
    public record CreateTaskDTO(string Name, string Description, TaskModelStatus Status, string? AssignedTo);
}
