using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskMind.Application.DTOs.TaskAssignments;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Controllers;

namespace TaskMind.Web.Tests
{
    public class TaskAssignControllerTests
    {
        private readonly Mock<ITaskAssignmentService> _mockTaskAssignmentService;
        private readonly TaskAssignController _controller;

        public TaskAssignControllerTests()
        {
            _mockTaskAssignmentService = new Mock<ITaskAssignmentService>();
            _controller = new TaskAssignController(_mockTaskAssignmentService.Object);
        }

        #region AssignEmployee Tests

        [Fact]
        public async Task AssignEmployee_WithSuccessfulAssignment_ShouldRedirectToManageTasks()
        {
            // Arrange
            var taskId = 1;
            var employeeId = 5;
            var teamId = 2;
            var successResult = TaskAssignmentResult.Successful(employeeId, teamId);

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(taskId))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.AssignEmployee(taskId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("ManageTasks");
            redirectResult.ControllerName.Should().Be("Teams");
            redirectResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(teamId);

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task AssignEmployee_NotFountProperEmployee_ShouldRedirectToManageTasks()
        {
            // Arrange
            var taskId = 1;
            var teamId = 2;
            var successResult = TaskAssignmentResult.Successful(null, teamId);

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(taskId))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.AssignEmployee(taskId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("ManageTasks");
            redirectResult.ControllerName.Should().Be("Teams");
            redirectResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(teamId);

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task AssignEmployee_WithFailedAssignment_ShouldReturnBadRequest()
        {
            // Arrange
            var taskId = 1;
            var errorMessage = "No available employees with required skills";
            var failedResult = TaskAssignmentResult.Failed(errorMessage);

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(taskId))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.AssignEmployee(taskId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var responseValue = badRequestResult.Value;
            responseValue.Should().BeEquivalentTo(new { message = errorMessage });

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task AssignEmployee_WhenTaskNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var taskId = 999;
            var exceptionMessage = "Task with ID 999 not found";

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(taskId))
                .ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            // Act
            var result = await _controller.AssignEmployee(taskId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var responseValue = notFoundResult.Value;
            responseValue.Should().BeEquivalentTo(new { message = exceptionMessage });

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task AssignEmployee_WhenServiceThrowsGeneralException_ShouldReturnInternalServerError()
        {
            // Arrange
            var taskId = 1;

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.AssignEmployee(taskId);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task AssignEmployee_WhenServiceThrowsSystemException_ShouldReturnInternalServerError()
        {
            // Arrange
            var taskId = 1;

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(taskId))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.AssignEmployee(taskId);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(taskId), Times.Once);
        }

        #endregion

        #region UnassignEmployee Tests

        [Fact]
        public async Task UnassignEmployee_WithSuccessfulUnassignment_ShouldRedirectToManageTasks()
        {
            // Arrange
            var taskId = 1;
            var teamId = 2;
            var successResult = TaskAssignmentResult.Successful(null, teamId);

            _mockTaskAssignmentService
                .Setup(s => s.UnassignEmployeeAsync(taskId))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.UnassignEmployee(taskId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("ManageTasks");
            redirectResult.ControllerName.Should().Be("Teams");
            redirectResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(teamId);

            _mockTaskAssignmentService.Verify(s => s.UnassignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task UnassignEmployee_WithFailedUnassignment_ShouldReturnBadRequest()
        {
            // Arrange
            var taskId = 1;
            var errorMessage = "Task is not assigned to any employee";
            var failedResult = TaskAssignmentResult.Failed(errorMessage);

            _mockTaskAssignmentService
                .Setup(s => s.UnassignEmployeeAsync(taskId))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.UnassignEmployee(taskId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var responseValue = badRequestResult.Value;
            responseValue.Should().BeEquivalentTo(new { message = errorMessage });

            _mockTaskAssignmentService.Verify(s => s.UnassignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task UnassignEmployee_WhenTaskNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var taskId = 999;
            var exceptionMessage = "Task with ID 999 not found";

            _mockTaskAssignmentService
                .Setup(s => s.UnassignEmployeeAsync(taskId))
                .ThrowsAsync(new KeyNotFoundException(exceptionMessage));

            // Act
            var result = await _controller.UnassignEmployee(taskId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var responseValue = notFoundResult.Value;
            responseValue.Should().BeEquivalentTo(new { message = exceptionMessage });

            _mockTaskAssignmentService.Verify(s => s.UnassignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task UnassignEmployee_WhenServiceThrowsGeneralException_ShouldReturnInternalServerError()
        {
            // Arrange
            var taskId = 1;

            _mockTaskAssignmentService
                .Setup(s => s.UnassignEmployeeAsync(taskId))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _controller.UnassignEmployee(taskId);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });

            _mockTaskAssignmentService.Verify(s => s.UnassignEmployeeAsync(taskId), Times.Once);
        }

        [Fact]
        public async Task UnassignEmployee_WhenServiceThrowsSystemException_ShouldReturnInternalServerError()
        {
            // Arrange
            var taskId = 1;

            _mockTaskAssignmentService
                .Setup(s => s.UnassignEmployeeAsync(taskId))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.UnassignEmployee(taskId);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().BeEquivalentTo(new { message = "Internal server error" });

            _mockTaskAssignmentService.Verify(s => s.UnassignEmployeeAsync(taskId), Times.Once);
        }

        #endregion

        #region Edge Cases Tests

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task AssignEmployee_WithInvalidTaskId_WhenServiceHandlesIt_ShouldReturnAppropriateResponse(int invalidTaskId)
        {
            // Arrange
            var errorMessage = $"Invalid task ID: {invalidTaskId}";
            var failedResult = TaskAssignmentResult.Failed(errorMessage);

            _mockTaskAssignmentService
                .Setup(s => s.AssignEmployeeAsync(invalidTaskId))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.AssignEmployee(invalidTaskId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var responseValue = badRequestResult.Value;
            responseValue.Should().BeEquivalentTo(new { message = errorMessage });

            _mockTaskAssignmentService.Verify(s => s.AssignEmployeeAsync(invalidTaskId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task UnassignEmployee_WithInvalidTaskId_WhenServiceHandlesIt_ShouldReturnAppropriateResponse(int invalidTaskId)
        {
            // Arrange
            var errorMessage = $"Invalid task ID: {invalidTaskId}";
            var failedResult = TaskAssignmentResult.Failed(errorMessage);

            _mockTaskAssignmentService
                .Setup(s => s.UnassignEmployeeAsync(invalidTaskId))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _controller.UnassignEmployee(invalidTaskId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var responseValue = badRequestResult.Value;
            responseValue.Should().BeEquivalentTo(new { message = errorMessage });

            _mockTaskAssignmentService.Verify(s => s.UnassignEmployeeAsync(invalidTaskId), Times.Once);
        }

        #endregion
    }
}
