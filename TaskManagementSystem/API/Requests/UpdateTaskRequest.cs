namespace API.Requests
{
    public record UpdateTaskRequest(int Id, string? Name, string? Description, int? Status);
}
