using Moq;
using BusinessLogic.Services;
using BusinessLogic.Interfaces.Repositories;
using Domain.DTO;
using Domain.Models;
using Domain.Enums;
using Infrastructure.MessageBroker;

namespace BusinessLogic.Tests.Services
{
    [TestFixture]
    public class TaskServiceTests
    {
        private Mock<ITaskRepository> _mockTaskRepository;
        private Mock<IMessageProduser> _mockMessageProduser;
        private TaskService _taskService;

        [SetUp]
        public void SetUp()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockMessageProduser = new Mock<IMessageProduser>();
            _taskService = new TaskService(_mockTaskRepository.Object, _mockMessageProduser.Object);
        }

        [Test]
        public async Task CreateTaskAsync_ShouldCreateTaskAndPublishMessage()
        {
            // Arrange
            var request = new CreateTaskDTO(
                Name: "Test Task",
                Description: "Description",
                Status: TaskModelStatus.NotStarted, // Use the enum here
                AssignedTo: "User1"
            );
            var expectedId = 1;

            _mockTaskRepository
                .Setup(repo => repo.CreateAsync(It.IsAny<TaskModel>()))
                .ReturnsAsync(expectedId);

            // Act
            var response = await _taskService.CreateTaskAsync(request);

            // Assert
            Assert.That(expectedId == response.Id);
            _mockMessageProduser.Verify(m => m.PublishTaskCreatedAsync(expectedId), Times.Once);
        }

        [Test]
        public async Task UpdateTaskAsync_ShouldUpdateTaskAndPublishMessage()
        {
            // Arrange
            var request = new UpdateTaskDTO(
                Id: 1,
                Name: "Updated Task",
                Description: "Updated Description",
                Status: TaskModelStatus.InProgress // Use the enum here
            );

            var task = new TaskModel
            {
                Id = 1,
                Name = "Old Task",
                Description = "Old Description",
                Status = TaskModelStatus.NotStarted, // Use the enum here
                AssignedTo = "User1"
            };

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(request.Id))
                .ReturnsAsync(task);

            _mockTaskRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _taskService.UpdateTaskAsync(request);

            // Assert
            Assert.That("Updated Task" == task.Name);
            Assert.That("Updated Description" == task.Description);
            Assert.That(TaskModelStatus.InProgress == task.Status); // Assert using enum
            _mockMessageProduser.Verify(m => m.PublishTaskUpdatedAsync(request.Id), Times.Once);
            _mockTaskRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateTaskAsync_ShouldThrowException_WhenTaskNotFound()
        {
            // Arrange
            var request = new UpdateTaskDTO(
                Id: 999, // non-existent task ID
                Name: "Non-existent Task",
                Description: null,
                Status: null
            );

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(request.Id))
                .ReturnsAsync((TaskModel)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _taskService.UpdateTaskAsync(request));
            Assert.That("Task is not found" == exception.Message);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            var task = new TaskModel
            {
                Id = 1,
                Name = "Test Task",
                Description = "Test Description",
                Status = TaskModelStatus.NotStarted, // Use the enum here
                AssignedTo = "User1"
            };

            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(1))
                .ReturnsAsync(task);

            // Act
            var response = await _taskService.GetByIdAsync(1);

            // Assert
            Assert.That(task == response.Task);
        }

        [Test]
        public async Task GetByIdAsync_ShouldThrowException_WhenTaskNotFound()
        {
            // Arrange
            _mockTaskRepository.Setup(repo => repo.GetTaskByIdAsync(999))
                .ReturnsAsync((TaskModel)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<KeyNotFoundException>(async () => await _taskService.GetByIdAsync(999));
            Assert.That("Task is not found" == exception.Message);
        }

        [Test]
        public async Task Search_ShouldReturnSearchResults()
        {
            // Arrange
            var searchRequest = new SearchTaskDTO(
                PageSize: 10,
                PageNumber: 1,
                Name: "Task",
                Description: null,
                Status: TaskModelStatus.Completed,
                AssignedTo: null,
                SearchFilter: null
            );
            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = 1, Name = "Task 1", Description = "Description", Status = TaskModelStatus.Completed },
                new TaskModel { Id = 2, Name = "Task 2", Description = "Description", Status = TaskModelStatus.Completed }
            };

            _mockTaskRepository.Setup(repo => repo.SearchAsync(searchRequest))
                .ReturnsAsync(tasks);

            // Act
            var response = await _taskService.Search(searchRequest);

            // Assert
            Assert.That(tasks.Count == response.TaskModels.Count);
        }
    }
}
