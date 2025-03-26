using BusinessLogic.Interfaces.Services;
using Domain.DTO;
using Infrastructure.MessageBroker;
using System.Text.Json;

namespace BusinessLogic.Services.ConsumerHandlers
{
    public class SearchTaskHandler : IConsumerHandler
    {
        private readonly ITaskService _taskService;
        private readonly IMessageProduser _messageProduser;

        public SearchTaskHandler(ITaskService taskService, IMessageProduser messageProduser)
        {
            _taskService = taskService;
            _messageProduser = messageProduser;
        }

        public async Task Handle(string message)
        {
            var parsedMessage = JsonSerializer.Deserialize<BrokerTaskDTO<SearchTaskDTO>>(message);
            var responce = await _taskService.Search(parsedMessage.Request);

            await _messageProduser.PublishTaskSearchResultAsync(parsedMessage.Id, responce.TaskModels);
        }
    }
}
