using FluentAssertions;
using Moq;
using TaskMind.Application.DTOs.TaskAssignments;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Tests
{
    public class TaskAssignmentServiceTests
    {
        private readonly Mock<ITaskAssignmentApi> _mockApi;
        private readonly Mock<ITaskItemRepository> _mockTaskItemRepo;
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
        private readonly Mock<IWorkloadCalculationService> _mockWorkloadService;
        private readonly TaskAssignmentService _taskAssignmentService;

        public TaskAssignmentServiceTests()
        {
            _mockApi = new Mock<ITaskAssignmentApi>();
            _mockTaskItemRepo = new Mock<ITaskItemRepository>();
            _mockEmployeeRepo = new Mock<IEmployeeRepository>();
            _mockWorkloadService = new Mock<IWorkloadCalculationService>();
            _taskAssignmentService = new TaskAssignmentService(
                _mockApi.Object,
                _mockTaskItemRepo.Object,
                _mockEmployeeRepo.Object,
                _mockWorkloadService.Object);
        }

        #region AssignEmployeeAsync Tests

        [Fact]
        public async Task AssignEmployeeAsync_WhenTaskNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var taskId = 999;
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync((TaskItem?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskAssignmentService.AssignEmployeeAsync(taskId));

            exception.Message.Should().Be("Task not found");
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenTaskAlreadyAssigned_ShouldReturnFailedResult()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = 5, TeamId = 1 };
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Task is already assigned");
            _mockEmployeeRepo.Verify(x => x.GetByTeamIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenNoEmployeesInTeam_ShouldReturnFailedResult()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = null, TeamId = 1 };
            var emptyEmployees = new List<Employee>();

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByTeamIdAsync(task.TeamId))
                            .ReturnsAsync(emptyEmployees);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No employees available in the team");
            _mockApi.Verify(x => x.GetBestDeveloper(It.IsAny<TaskRequestDto>()), Times.Never);
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenApiReturnsNull_ShouldReturnFailedResult()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = null, TeamId = 1 };
            var employees = new List<Employee>
        {
            new() { Id = 1, Name = "John", TeamId = 1 }
        };

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByTeamIdAsync(task.TeamId))
                            .ReturnsAsync(employees);
            _mockApi.Setup(x => x.GetBestDeveloper(It.IsAny<TaskRequestDto>()))
                  .ReturnsAsync((BestDeveloperResponse?)null);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No suitable developer found");
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenApiReturnsBestDeveloperNull_ShouldReturnFailedResult()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = null, TeamId = 1 };
            var employees = new List<Employee>
        {
            new() { Id = 1, Name = "John", TeamId = 1 }
        };
            var apiResponse = new BestDeveloperResponse { BestDeveloper = null };

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByTeamIdAsync(task.TeamId))
                            .ReturnsAsync(employees);
            _mockApi.Setup(x => x.GetBestDeveloper(It.IsAny<TaskRequestDto>()))
                  .ReturnsAsync(apiResponse);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No suitable developer found");
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenSelectedEmployeeNotFound_ShouldReturnFailedResult()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = null, TeamId = 1 };
            var employees = new List<Employee>
        {
            new() { Id = 1, Name = "John", TeamId = 1 }
        };
            var apiResponse = new BestDeveloperResponse
            {
                BestDeveloper = new DeveloperResponse { DeveloperID = 999 }
            };

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByTeamIdAsync(task.TeamId))
                            .ReturnsAsync(employees);
            _mockApi.Setup(x => x.GetBestDeveloper(It.IsAny<TaskRequestDto>()))
                  .ReturnsAsync(apiResponse);
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(999))
                            .ReturnsAsync((Employee?)null);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Selected developer not found");
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenSuccessful_ShouldAssignTaskAndUpdateWorkload()
        {
            // Arrange
            var taskId = 1;
            var employeeId = 1;
            var teamId = 1;
            var task = new TaskItem
            {
                Id = taskId,
                EmployeeId = null,
                TeamId = teamId,
                EstimatedHours = 10,
                Difficulty = 5,
                DeadlineDays = 7
            };
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John",
                TeamId = teamId,
                CurrentWorkload = 0.5,
                TaskCompletionSpeed = 8.0
            };
            var employees = new List<Employee> { employee };
            var apiResponse = new BestDeveloperResponse
            {
                BestDeveloper = new DeveloperResponse { DeveloperID = employeeId }
            };
            var workloadDelta = 0.2;

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByTeamIdAsync(teamId))
                            .ReturnsAsync(employees);
            _mockApi.Setup(x => x.GetBestDeveloper(It.IsAny<TaskRequestDto>()))
                  .ReturnsAsync(apiResponse);
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(employee);
            _mockWorkloadService.Setup(x => x.CalculateWorkloadChange(task, employee))
                              .Returns(workloadDelta);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AssignedEmployeeId.Should().Be(employeeId);
            result.TeamId.Should().Be(teamId);

            // Verify task assignment
            task.EmployeeId.Should().Be(employeeId);
            _mockTaskItemRepo.Verify(x => x.UpdateAsync(task), Times.Once);

            // Verify workload update
            employee.CurrentWorkload.Should().Be(0.7); // 0.5 + 0.2
            _mockEmployeeRepo.Verify(x => x.UpdateAsync(employee), Times.Once);
        }

        [Fact]
        public async Task AssignEmployeeAsync_WhenWorkloadExceedsOne_ShouldCapAtOne()
        {
            // Arrange
            var taskId = 1;
            var employeeId = 1;
            var teamId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = null, TeamId = teamId };
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John",
                TeamId = teamId,
                CurrentWorkload = 0.8
            };
            var employees = new List<Employee> { employee };
            var apiResponse = new BestDeveloperResponse
            {
                BestDeveloper = new DeveloperResponse { DeveloperID = employeeId }
            };
            var workloadDelta = 0.5; // Would result in 1.3

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByTeamIdAsync(teamId))
                            .ReturnsAsync(employees);
            _mockApi.Setup(x => x.GetBestDeveloper(It.IsAny<TaskRequestDto>()))
                  .ReturnsAsync(apiResponse);
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(employee);
            _mockWorkloadService.Setup(x => x.CalculateWorkloadChange(task, employee))
                              .Returns(workloadDelta);

            // Act
            var result = await _taskAssignmentService.AssignEmployeeAsync(taskId);

            // Assert
            result.Success.Should().BeTrue();
            employee.CurrentWorkload.Should().Be(1.0); // Capped at 1
        }

        #endregion

        #region UnassignEmployeeAsync Tests

        [Fact]
        public async Task UnassignEmployeeAsync_WhenTaskNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var taskId = 999;
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync((TaskItem?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _taskAssignmentService.UnassignEmployeeAsync(taskId));

            exception.Message.Should().Be("Task not found");
        }

        [Fact]
        public async Task UnassignEmployeeAsync_WhenTaskNotAssigned_ShouldReturnFailedResult()
        {
            // Arrange
            var taskId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = null, TeamId = 1 };
            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);

            // Act
            var result = await _taskAssignmentService.UnassignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("Task is not assigned to any employee");
        }

        [Fact]
        public async Task UnassignEmployeeAsync_WhenEmployeeExists_ShouldUnassignAndUpdateWorkload()
        {
            // Arrange
            var taskId = 1;
            var employeeId = 1;
            var teamId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = employeeId, TeamId = teamId };
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John",
                CurrentWorkload = 0.7
            };
            var workloadDelta = 0.2;

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(employee);
            _mockWorkloadService.Setup(x => x.CalculateWorkloadChange(task, employee))
                              .Returns(workloadDelta);

            // Act
            var result = await _taskAssignmentService.UnassignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AssignedEmployeeId.Should().BeNull();
            result.TeamId.Should().Be(teamId);

            // Verify task unassignment
            task.EmployeeId.Should().BeNull();
            _mockTaskItemRepo.Verify(x => x.UpdateAsync(task), Times.Once);

            // Verify workload update
            employee.CurrentWorkload.Should().Be(0.5); // 0.7 - 0.2
            _mockEmployeeRepo.Verify(x => x.UpdateAsync(employee), Times.Once);
        }

        [Fact]
        public async Task UnassignEmployeeAsync_WhenEmployeeNotFound_ShouldStillUnassignTask()
        {
            // Arrange
            var taskId = 1;
            var employeeId = 999;
            var teamId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = employeeId, TeamId = teamId };

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync((Employee?)null);

            // Act
            var result = await _taskAssignmentService.UnassignEmployeeAsync(taskId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AssignedEmployeeId.Should().BeNull();
            result.TeamId.Should().Be(teamId);

            // Verify task unassignment
            task.EmployeeId.Should().BeNull();
            _mockTaskItemRepo.Verify(x => x.UpdateAsync(task), Times.Once);

            // Verify no workload update
            _mockEmployeeRepo.Verify(x => x.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        }

        [Fact]
        public async Task UnassignEmployeeAsync_WhenWorkloadWouldBeNegative_ShouldSetToZero()
        {
            // Arrange
            var taskId = 1;
            var employeeId = 1;
            var teamId = 1;
            var task = new TaskItem { Id = taskId, EmployeeId = employeeId, TeamId = teamId };
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John",
                CurrentWorkload = 0.1
            };
            var workloadDelta = 0.2; // Would result in -0.1

            _mockTaskItemRepo.Setup(x => x.GetByIdAsync(taskId))
                            .ReturnsAsync(task);
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(employee);
            _mockWorkloadService.Setup(x => x.CalculateWorkloadChange(task, employee))
                              .Returns(workloadDelta);

            // Act
            var result = await _taskAssignmentService.UnassignEmployeeAsync(taskId);

            // Assert
            result.Success.Should().BeTrue();
            employee.CurrentWorkload.Should().Be(0.0); // Capped at 0
        }

        #endregion
    }
}
