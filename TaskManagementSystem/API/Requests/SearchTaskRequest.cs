namespace API.Requests
{
    public record SearchTaskRequest(int PageSize, int PageNumber, string? Name, string? Description, int? Status, string? AssignedTo, string? SearchQuery);
}
