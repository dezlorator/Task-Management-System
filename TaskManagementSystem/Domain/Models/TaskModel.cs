using Domain.Enums;

namespace Domain.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public TaskModelStatus Status { get; set; }
        public string? AssignedTo { get; set; }
    }
}
