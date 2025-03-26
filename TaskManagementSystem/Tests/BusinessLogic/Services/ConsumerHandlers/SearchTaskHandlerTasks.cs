using BusinessLogic.Interfaces.Services;
using BusinessLogic.Services.ConsumerHandlers;
using Domain.DTO;
using Domain.Enums;
using Domain.Models;
using Domain.Responses;
using Infrastructure.MessageBroker;
using Moq;
using System.Text.Json;

namespace BusinessLogic.Tests.Services.ConsumerHandlers
{
    [TestFixture]
    public class SearchTaskHandlerTests
    {
        private Mock<ITaskService> _mockTaskService;
        private Mock<IMessageProduser> _mockMessageProduser;
        private SearchTaskHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _mockTaskService = new Mock<ITaskService>();
            _mockMessageProduser = new Mock<IMessageProduser>();
            _handler = new SearchTaskHandler(_mockTaskService.Object, _mockMessageProduser.Object);
        }

        [Test]
        public async Task Handle_ValidMessage_SearchesTaskAndPublishesResult()
        {
            // Arrange
            var request = new SearchTaskDTO(
                PageSize: 10,
                PageNumber: 1,
                Name: "Test Task",
                Description: "A sample task",
                Status: TaskModelStatus.InProgress,
                AssignedTo: "User123",
                SearchFilter: "urgent"
            );

            var response = new SearchTaskResponce(new List<TaskModel>
            {
                new TaskModel
                {
                    Id = 1,
                    Name = "Test Task",
                    Description = "A sample task",
                    Status = TaskModelStatus.InProgress,
                    AssignedTo = "User123"
                }
            });

            var brokerMessage = new BrokerTaskDTO<SearchTaskDTO>
            {
                Id = Guid.NewGuid(),
                Request = request
            };

            var serializedMessage = JsonSerializer.Serialize(brokerMessage);

            _mockTaskService.Setup(s => s.Search(request)).ReturnsAsync(response);

            // Act
            await _handler.Handle(serializedMessage);

            // Assert
            _mockTaskService.Verify(s => s.Search(request), Times.Once);
            _mockMessageProduser.Verify(m => m.PublishTaskSearchResultAsync(brokerMessage.Id, response.TaskModels), Times.Once);
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
            var request = new SearchTaskDTO(
                PageSize: 5,
                PageNumber: 2,
                Name: null,
                Description: "Testing failure",
                Status: null,
                AssignedTo: "TestUser",
                SearchFilter: null
            );

            var brokerMessage = new BrokerTaskDTO<SearchTaskDTO>
            {
                Id = Guid.NewGuid(),
                Request = request
            };

            var serializedMessage = JsonSerializer.Serialize(brokerMessage);

            _mockTaskService.Setup(s => s.Search(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(serializedMessage));
        }

        [Test]
        public async Task Handle_NullOptionalFields_DoesNotThrowException()
        {
            // Arrange
            var request = new SearchTaskDTO(
                PageSize: 10,
                PageNumber: 1,
                Name: null,
                Description: null,
                Status: null,
                AssignedTo: null,
                SearchFilter: null
            );

            var response = new SearchTaskResponce(new List<TaskModel>());

            var brokerMessage = new BrokerTaskDTO<SearchTaskDTO>
            {
                Id = Guid.NewGuid(),
                Request = request
            };

            var serializedMessage = JsonSerializer.Serialize(brokerMessage);

            _mockTaskService.Setup(s => s.Search(request)).ReturnsAsync(response);

            // Act
            await _handler.Handle(serializedMessage);

            // Assert
            _mockTaskService.Verify(s => s.Search(request), Times.Once);
            _mockMessageProduser.Verify(m => m.PublishTaskSearchResultAsync(brokerMessage.Id, response.TaskModels), Times.Once);
        }
    }
}
