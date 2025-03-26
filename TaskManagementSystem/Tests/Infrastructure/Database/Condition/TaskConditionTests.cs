using Domain.Enums;
using Domain.Models;
using Infrastructure.Database.Condition;

namespace Tests.Infrastructure.Database.Condition
{
    [TestFixture]
    public class TaskConditionsTests
    {
        private TaskConditions _taskConditions;
        private IQueryable<TaskModel> _taskList;

        [SetUp]
        public void Setup()
        {
            _taskConditions = new TaskConditions();

            _taskList = new List<TaskModel>
            {
                new TaskModel { Name = "Task 1", Description = "First task", AssignedTo = "User A", Status = TaskModelStatus.NotStarted },
                new TaskModel { Name = "Task 2", Description = "Second task", AssignedTo = "User B", Status = TaskModelStatus.Completed },
                new TaskModel { Name = "Another Task", Description = "Different", AssignedTo = "User A", Status = TaskModelStatus.InProgress }
            }.AsQueryable();
        }

        [Test]
        public void ApplySearchFilter_ShouldReturnMatchingTasks()
        {
            var result = _taskConditions.ApplySearchFilter(_taskList, "Another");

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ApplySearchFilter_ShouldReturnAllTasks_WhenFilterIsEmpty()
        {
            var result = _taskConditions.ApplySearchFilter(_taskList, "");

            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public void ApplyPagination_ShouldReturnPaginatedTasks()
        {
            var result = _taskConditions.ApplyPagination(_taskList, 1, 2);

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Task 2"));
        }

        [Test]
        public void ApplyPagination_ShouldReturnEmpty_WhenStartIndexExceedsList()
        {
            var result = _taskConditions.ApplyPagination(_taskList, 5, 2);

            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ApplyNameCondition_ShouldReturnTasksWithMatchingName()
        {
            var result = _taskConditions.ApplyNameCondition(_taskList, "Task 1");

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ApplyNameCondition_ShouldReturnAllTasks_WhenNameIsEmpty()
        {
            var result = _taskConditions.ApplyNameCondition(_taskList, null);

            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public void ApplyDescriptionCondition_ShouldReturnTasksWithMatchingDescription()
        {
            var result = _taskConditions.ApplyDescriptionCondition(_taskList, "Different");

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ApplyAssignedToCondition_ShouldReturnTasksAssignedToUserA()
        {
            var result = _taskConditions.ApplyAssignedToCondition(_taskList, "User A");

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void ApplyStatusCondition_ShouldReturnTasksWithNotStartedStatus()
        {
            var result = _taskConditions.ApplyStatusCondition(_taskList, TaskModelStatus.NotStarted);

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ApplyStatusCondition_ShouldReturnTasksWithCompletedStatus()
        {
            var result = _taskConditions.ApplyStatusCondition(_taskList, TaskModelStatus.Completed);

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ApplyStatusCondition_ShouldReturnTasksWithInProgressStatus()
        {
            var result = _taskConditions.ApplyStatusCondition(_taskList, TaskModelStatus.InProgress);

            Assert.That(result.Count(), Is.EqualTo(1));
        }

        [Test]
        public void ApplyStatusCondition_ShouldReturnAllTasks_WhenStatusIsNull()
        {
            var result = _taskConditions.ApplyStatusCondition(_taskList, null);

            Assert.That(result.Count(), Is.EqualTo(3));
        }
    }
}
