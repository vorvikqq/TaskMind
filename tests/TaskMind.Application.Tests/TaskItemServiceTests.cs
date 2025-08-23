using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services;
using TaskMind.Domain.Constants;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Tests
{
    public class TaskItemServiceTests
    {
        private readonly Mock<ITaskItemRepository> _mockTaskItemRepo;
        private readonly Mock<ITeamRepository> _mockTeamRepo;
        private readonly TaskItemService _taskItemService;

        public TaskItemServiceTests()
        {
            _mockTaskItemRepo = new Mock<ITaskItemRepository>();
            _mockTeamRepo = new Mock<ITeamRepository>();
            _taskItemService = new TaskItemService(_mockTaskItemRepo.Object, _mockTeamRepo.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_ShouldInitializeTaskStatesFromEnum()
        {
            // Arrange & Act
            var service = new TaskItemService(_mockTaskItemRepo.Object, _mockTeamRepo.Object);

            // Assert - using reflection to access private field
            var taskStatesField = typeof(TaskItemService)
                .GetField("_taskStates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var taskStates = (List<SelectListItem>)taskStatesField!.GetValue(service)!;

            taskStates.Should().NotBeEmpty();
            var expectedStatesCount = Enum.GetValues(typeof(TaskState)).Length;
            taskStates.Should().HaveCount(expectedStatesCount);

            // Check that enum values are properly converted
            foreach (TaskState state in Enum.GetValues(typeof(TaskState)))
            {
                taskStates.Should().Contain(item =>
                    item.Value == ((int)state).ToString() &&
                    item.Text == state.ToString());
            }
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTaskItems()
        {
            // Arrange
            var expectedTasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", Description = "Description 1" },
            new() { Id = 2, Title = "Task 2", Description = "Description 2" }
        };
            _mockTaskItemRepo.Setup(x => x.GetAllAsync())
                            .ReturnsAsync(expectedTasks);

            // Act
            var result = await _taskItemService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedTasks);
            _mockTaskItemRepo.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTasks_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockTaskItemRepo.Setup(x => x.GetAllAsync())
                            .ReturnsAsync(new List<TaskItem>());

            // Act
            var result = await _taskItemService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTaskExists_ShouldReturnTask()
        {
            // Arrange
            var taskId = 1;
            var expectedTask = new TaskItem { Id = taskId, Title = "Test Task" };
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(expectedTask);

            // Act
            var result = await _taskItemService.GetByIdAsync(taskId);

            // Assert
            result.Should().BeEquivalentTo(expectedTask);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTaskNotExists_ShouldReturnNull()
        {
            // Arrange
            var taskId = 999;
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _taskItemService.GetByIdAsync(taskId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetCreateModelAsync Tests

        [Fact]
        public async Task GetCreateModelAsync_WithoutDto_ShouldReturnModelWithNewDto()
        {
            // Arrange
            var teams = new List<Team>
        {
            new() { Id = 1, Name = "Team 1" },
            new() { Id = 2, Name = "Team 2" }
        };
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _taskItemService.GetCreateModelAsync();

            // Assert
            result.Should().NotBeNull();
            result.TaskItem.Should().NotBeNull();
            result.TaskItem.Should().BeOfType<CreateTaskItemRequest>();
            result.Teams.Should().NotBeNull();
            result.TaskStates.Should().NotBeNull();
            result.TaskStates.Should().NotBeEmpty();

            var teamItems = result.Teams.Items.Cast<Team>().ToList();
            teamItems.Should().HaveCount(2);
            teamItems.Should().Contain(t => t.Name == "Team 1");
            teamItems.Should().Contain(t => t.Name == "Team 2");
        }

        [Fact]
        public async Task GetCreateModelAsync_WithDto_ShouldReturnModelWithProvidedDto()
        {
            // Arrange
            var teams = new List<Team>
        {
            new() { Id = 1, Name = "Team 1" },
            new() { Id = 2, Name = "Team 2" }
        };
            var dto = new CreateTaskItemRequest
            {
                Title = "Test Task",
                TeamId = 2
            };
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _taskItemService.GetCreateModelAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.TaskItem.Should().BeSameAs(dto);
            result.Teams.SelectedValue.Should().Be(2);
        }

        [Fact]
        public async Task GetCreateModelAsync_ShouldIncludeAllTaskStates()
        {
            // Arrange
            var teams = new List<Team> { new() { Id = 1, Name = "Team 1" } };
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _taskItemService.GetCreateModelAsync();

            // Assert
            var expectedStatesCount = Enum.GetValues(typeof(TaskState)).Length;
            result.TaskStates.Should().HaveCount(expectedStatesCount);

            foreach (TaskState state in Enum.GetValues(typeof(TaskState)))
            {
                result.TaskStates.Should().Contain(item =>
                    item.Value == ((int)state).ToString() &&
                    item.Text == state.ToString());
            }
        }

        #endregion

        #region GetEditModelAsync Tests

        [Fact]
        public async Task GetEditModelAsync_WhenTaskExists_WithoutDto_ShouldReturnModelWithConvertedDto()
        {
            // Arrange
            var taskId = 1;
            var taskItem = new TaskItem
            {
                Id = taskId,
                Title = "Existing Task",
                Description = "Task Description",
                Difficulty = 5,
                RequiredSkills = new List<string> { "C#", "React" },
                DeadlineDays = 10,
                EstimatedHours = 40,
                TeamId = 2
            };
            var teams = new List<Team>
        {
            new() { Id = 1, Name = "Team 1" },
            new() { Id = 2, Name = "Team 2" }
        };

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(taskItem);
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _taskItemService.GetEditModelAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result!.TaskItem.Should().NotBeNull();
            result.TaskItem.Id.Should().Be(taskId);
            result.TaskItem.Title.Should().Be("Existing Task");
            result.TaskItem.Description.Should().Be("Task Description");
            result.TaskItem.Difficulty.Should().Be(5);
            result.TaskItem.RequiredSkills.Should().Be("C#, React");
            result.TaskItem.DeadlineDays.Should().Be(10);
            result.TaskItem.EstimatedHours.Should().Be(40);
            result.TaskItem.TeamId.Should().Be(2);

            result.Teams.SelectedValue.Should().Be(2);
            result.TaskStates.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetEditModelAsync_WhenTaskExists_WithDto_ShouldReturnModelWithProvidedDto()
        {
            // Arrange
            var taskId = 1;
            var taskItem = new TaskItem { Id = taskId, Title = "Existing Task" };
            var providedDto = new UpdateTaskItemRequest
            {
                Id = taskId,
                Title = "Updated Title",
                TeamId = 3
            };
            var teams = new List<Team> { new() { Id = 3, Name = "Team 3" } };

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(taskItem);
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _taskItemService.GetEditModelAsync(taskId, providedDto);

            // Assert
            result.Should().NotBeNull();
            result!.TaskItem.Should().BeSameAs(providedDto);
            result.Teams.SelectedValue.Should().Be(3);
        }

        [Fact]
        public async Task GetEditModelAsync_WhenTaskNotExists_ShouldReturnNull()
        {
            // Arrange
            var taskId = 999;
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _taskItemService.GetEditModelAsync(taskId);

            // Assert
            result.Should().BeNull();
            _mockTeamRepo.Verify(x => x.GetAllAsync(), Times.Never);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldCallRepositoryCreate()
        {
            // Arrange
            var createDto = new CreateTaskItemRequest
            {
                Title = "New Task",
                Description = "Task Description"
            };

            // Act
            await _taskItemService.CreateAsync(createDto);

            // Assert
            _mockTaskItemRepo.Verify(x => x.CreateAsync(It.IsAny<TaskItem>()), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenTaskExists_ShouldUpdateTask()
        {
            // Arrange
            var updateDto = new UpdateTaskItemRequest
            {
                Id = 1,
                Title = "Updated Task",
                Description = "Updated Description"
            };


            _mockTaskItemRepo.Setup(x => x.UpdateAsync(updateDto.Id, It.IsAny<TaskItem>()))
                            .ReturnsAsync(1);

            // Act
            await _taskItemService.UpdateAsync(updateDto);

            // Assert
            _mockTaskItemRepo.Verify(x => x.UpdateAsync(updateDto.Id, It.IsAny<TaskItem>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateTaskItemRequest { Id = 999 };

            _mockTaskItemRepo.Setup(x => x.UpdateAsync(updateDto.Id, It.IsAny<TaskItem>()))
                            .ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskItemService.UpdateAsync(updateDto));

            exception.Message.Should().Be("Task not found");
            _mockTaskItemRepo.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<TaskItem>()), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTaskExists_ShouldDeleteTask()
        {
            // Arrange
            var taskId = 1;
            var existingTask = new TaskItem { Id = taskId, Title = "Task to Delete" };
            _mockTaskItemRepo.Setup(x => x.DeleteAsync(taskId))
                            .ReturnsAsync(1);

            // Act
            await _taskItemService.DeleteAsync(taskId);

            // Assert
            _mockTaskItemRepo.Verify(x => x.DeleteAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var taskId = 999;
            _mockTaskItemRepo.Setup(x => x.DeleteAsync(taskId))
                            .ReturnsAsync(0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskItemService.DeleteAsync(taskId));

            exception.Message.Should().Be("Task not found");
            _mockTaskItemRepo.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenTaskExists_ShouldReturnTrue()
        {
            // Arrange
            var taskId = 1;
            _mockTaskItemRepo.Setup(x => x.ExistsAsync(taskId))
                            .ReturnsAsync(true);

            // Act
            var result = await _taskItemService.ExistsAsync(taskId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenTaskNotExists_ShouldReturnFalse()
        {
            // Arrange
            var taskId = 999;
            _mockTaskItemRepo.Setup(x => x.ExistsAsync(taskId))
                            .ReturnsAsync(false);

            // Act
            var result = await _taskItemService.ExistsAsync(taskId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
