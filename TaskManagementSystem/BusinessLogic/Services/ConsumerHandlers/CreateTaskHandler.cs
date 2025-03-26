using BusinessLogic.Interfaces.Services;
using Domain.DTO;
using Infrastructure.MessageBroker;
using System.Text.Json;

namespace BusinessLogic.Services.ConsumerHandlers
{
    public class CreateTaskHandler : IConsumerHandler
    {
        private readonly ITaskService _taskService;
        private readonly IMessageProduser _messageProduser;

        public CreateTaskHandler(ITaskService taskService, IMessageProduser messageProduser)
        {
            _taskService = taskService;
            _messageProduser = messageProduser;
        }

        public async Task Handle(string message)
        {
            var parsedMessage = JsonSerializer.Deserialize<BrokerTaskDTO<CreateTaskDTO>>(message);
            var responce = await _taskService.CreateTaskAsync(parsedMessage.Request);

            await _messageProduser.PublishTaskCreatedResultAsync(parsedMessage.Id, responce.Id);
        }
    }
}
