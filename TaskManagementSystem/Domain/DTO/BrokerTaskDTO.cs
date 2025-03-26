namespace Domain.DTO
{
    public class BrokerTaskDTO<T>
    {
        public Guid Id { get; set; }
        public required T Request {  get; set; } 
    }
}
