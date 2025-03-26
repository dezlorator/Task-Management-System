namespace BusinessLogic.Services.ConsumerHandlers
{
    public interface IConsumerHandler
    {
        Task Handle(string message);
    }
}
