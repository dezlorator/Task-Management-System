using BusinessLogic.Interfaces.Services;
using BusinessLogic.Services.ConsumerHandlers;
using Domain.DTO;
using Domain.Enums;
using Domain.Responses;
using Infrastructure.MessageBroker;
using Moq;
using System.Text.Json;

namespace BusinessLogic.Tests.Services.ConsumerHandlers
{
    [TestFixture]
    public class CreateTaskHandlerTests
    {
        private Mock<ITaskService> _mockTaskService;
        private Mock<IMessageProduser> _mockMessageProduser;
        private CreateTaskHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockMessageProduser = new Mock<IMessageProduser>();
            _handler = new CreateTaskHandler(_mockTaskService.Object, _mockMessageProduser.Object);
        }

        [Test]
        public async Task Handle_ValidMessage_CreatesTaskAndPublishesResult()
        {
            // Arrange
            var taskId = 1;
            var request = new CreateTaskDTO ("Test Task", "Desc", TaskModelStatus.InProgress, null);
            var response = new CreateTaskResponce(taskId);

            var brokerMessage = new BrokerTaskDTO<CreateTaskDTO>
            {
                Id = Guid.NewGuid(),
                Request = request
            };

            var serializedMessage = JsonSerializer.Serialize(brokerMessage);

            _mockTaskService.Setup(s => s.CreateTaskAsync(request)).ReturnsAsync(response);

            // Act
            await _handler.Handle(serializedMessage);

            // Assert
            _mockTaskService.Verify(s => s.CreateTaskAsync(request), Times.Once);
            _mockMessageProduser.Verify(m => m.PublishTaskCreatedResultAsync(brokerMessage.Id, response.Id), Times.Once);
        }

        [Test]
        public void Handle_NullMessage_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _handler.Handle(null));
        }

        [Test]
        public void Handle_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidMessage = "invalid json";

            // Act & Assert
            Assert.ThrowsAsync<JsonException>(async () => await _handler.Handle(invalidMessage));
        }

        [Test]
        public void Handle_TaskServiceThrowsException_ThrowsException()
        {
            // Arrange
            var request = new CreateTaskDTO("Test Task", "Desc", TaskModelStatus.InProgress, null);
            var brokerMessage = new BrokerTaskDTO<CreateTaskDTO>
            {
                Id = Guid.NewGuid(),
                Request = request
            };

            var serializedMessage = JsonSerializer.Serialize(brokerMessage);

            _mockTaskService.Setup(s => s.CreateTaskAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(serializedMessage));
        }
    }
}
