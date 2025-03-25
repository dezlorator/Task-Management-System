using Domain.Enums;

namespace Domain.DTO
{
    public record SearchTaskDTO(int PageSize, int PageNumber, string? Name, string? Description, TaskModelStatus? Status, string? AssignedTo, string? SearchFilter);
}
