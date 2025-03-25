namespace API.Requests
{
    public record CreateTaskRequest(string Name, string Description, int Status, string? AssignedTo);
}
