using Domain.Enums;

namespace Domain.DTO
{
    public record UpdateTaskDTO(int Id, string? Name, string? Description, TaskModelStatus? Status);
}
