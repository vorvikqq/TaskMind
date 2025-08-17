using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using TaskMind.Application.DTOs.TaskItem;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Controllers;
using TaskMind.Domain.Constants;
using TaskMind.Domain.Models;

namespace TaskMind.Web.Tests
{
    public class TaskItemsControllerTests
    {
        private readonly Mock<ITaskItemService> _mockTaskItemService;
        private readonly TaskItemsController _controller;

        public TaskItemsControllerTests()
        {
            _mockTaskItemService = new Mock<ITaskItemService>();
            _controller = new TaskItemsController(_mockTaskItemService.Object);

            var mockValidator = new Mock<IObjectModelValidator>();
            _controller.ObjectValidator = mockValidator.Object;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ShouldReturnViewWithTaskItems()
        {
            // Arrange
            var taskItems = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", TeamId = 1, Status = TaskState.New },
            new TaskItem { Id = 2, Title = "Task 2", TeamId = 2, Status = TaskState.InProgress }
        };

            _mockTaskItemService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(taskItems);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<TaskItem>>().Subject;
            model.Should().HaveCount(2);
            model.Should().Contain(t => t.Title == "Task 1");
            model.Should().Contain(t => t.Title == "Task 2");

            _mockTaskItemService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Index_WhenNoTaskItems_ShouldReturnViewWithEmptyList()
        {
            // Arrange
            _mockTaskItemService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<TaskItem>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<TaskItem>>().Subject;
            model.Should().BeEmpty();
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockTaskItemService
                .Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            var act = async () => await _controller.Index();
            await act.Should().ThrowAsync<Exception>().WithMessage("Service error");
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_WithValidId_ShouldReturnViewWithTaskItem()
        {
            // Arrange
            var taskItemId = 1;
            var taskItem = new TaskItem
            {
                Id = taskItemId,
                Title = "Test Task",
                Description = "Test Description",
                Difficulty = 0.5,
                RequiredSkills = new List<string> { "C#", "React" },
                DeadlineDays = 5,
                EstimatedHours = 10,
                Status = TaskState.New,
                TeamId = 1
            };

            _mockTaskItemService
                .Setup(s => s.GetByIdAsync(taskItemId))
                .ReturnsAsync(taskItem);

            // Act
            var result = await _controller.Details(taskItemId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TaskItem>().Subject;
            model.Id.Should().Be(taskItemId);
            model.Title.Should().Be("Test Task");
            model.Description.Should().Be("Test Description");
            model.Difficulty.Should().Be(0.5);

            _mockTaskItemService.Verify(s => s.GetByIdAsync(taskItemId), Times.Once);
        }

        [Fact]
        public async Task Details_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Details_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var taskItemId = 999;
            _mockTaskItemService
                .Setup(s => s.GetByIdAsync(taskItemId))
                .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _controller.Details(taskItemId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.GetByIdAsync(taskItemId), Times.Once);
        }

        [Fact]
        public async Task Details_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var taskItemId = 1;
            _mockTaskItemService
                .Setup(s => s.GetByIdAsync(taskItemId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var act = async () => await _controller.Details(taskItemId);
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_GET_ShouldReturnViewWithCreateModel()
        {
            // Arrange
            var teamsData = new List<Team>
        {
            new Team { Id = 1, Name = "Development Team" },
            new Team { Id = 2, Name = "QA Team" }
        };

            var taskStates = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = "Not Started" },
            new SelectListItem { Value = "1", Text = "In Progress" }
        };

            var createModel = new TaskItemCreateModel
            {
                TaskItem = new CreateTaskItemDto(),
                Teams = new SelectList(teamsData, "Id", "Name"),
                TaskStates = taskStates
            };

            _mockTaskItemService
                .Setup(s => s.GetCreateModelAsync(null))
                .ReturnsAsync(createModel);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TaskItemCreateModel>().Subject;
            model.Teams.Should().HaveCount(2);
            model.TaskStates.Should().HaveCount(2);

            _mockTaskItemService.Verify(s => s.GetCreateModelAsync(null), Times.Once);
        }

        [Fact]
        public async Task Create_GET_WhenNoTeams_ShouldReturnViewWithEmptyTeamsDropdown()
        {
            // Arrange
            var createModel = new TaskItemCreateModel
            {
                TaskItem = new CreateTaskItemDto(),
                Teams = new SelectList(Enumerable.Empty<SelectListItem>()),
                TaskStates = new List<SelectListItem>()
            };

            _mockTaskItemService
                .Setup(s => s.GetCreateModelAsync(null))
                .ReturnsAsync(createModel);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TaskItemCreateModel>().Subject;
            model.Teams.Should().BeEmpty();
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_POST_WithValidModel_ShouldCreateTaskItemAndRedirectToIndex()
        {
            // Arrange
            var createDto = new CreateTaskItemDto
            {
                Title = "New Task",
                Description = "Task Description",
                Difficulty = 0.7,
                RequiredSkills = "C#, React",
                DeadlineDays = 5,
                EstimatedHours = 10,
                Status = TaskState.New,
                TeamId = 1
            };

            var model = new TaskItemCreateModel
            {
                TaskItem = createDto
            };

            _mockTaskItemService
                .Setup(s => s.CreateAsync(createDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockTaskItemService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_POST_WithInvalidModel_ShouldReturnViewWithModelAndDropdowns()
        {
            // Arrange
            var createDto = new CreateTaskItemDto
            {
                Title = "", // Invalid - empty title
                TeamId = 1
            };

            var model = new TaskItemCreateModel
            {
                TaskItem = createDto
            };

            var teamsData = new List<Team>
        {
            new Team { Id = 1, Name = "Development Team" }
        };

            var createModelWithDropdowns = new TaskItemCreateModel
            {
                TaskItem = createDto,
                Teams = new SelectList(teamsData, "Id", "Name"),
                TaskStates = new List<SelectListItem>()
            };

            _mockTaskItemService
                .Setup(s => s.GetCreateModelAsync(createDto))
                .ReturnsAsync(createModelWithDropdowns);

            _controller.ModelState.AddModelError("TaskItem.Title", "Title is required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var resultModel = viewResult.Model.Should().BeAssignableTo<TaskItemCreateModel>().Subject;
            resultModel.TaskItem.Should().BeEquivalentTo(createDto);
            resultModel.Teams.Should().HaveCount(1);

            _mockTaskItemService.Verify(s => s.CreateAsync(It.IsAny<CreateTaskItemDto>()), Times.Never);
            _mockTaskItemService.Verify(s => s.GetCreateModelAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_POST_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var createDto = new CreateTaskItemDto
            {
                Title = "Test Task",
                Description = "Test Description",
                TeamId = 1
            };

            var model = new TaskItemCreateModel
            {
                TaskItem = createDto
            };

            _mockTaskItemService
                .Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new Exception("Creation failed"));

            // Act & Assert
            var act = async () => await _controller.Create(model);
            await act.Should().ThrowAsync<Exception>().WithMessage("Creation failed");
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_GET_WithValidId_ShouldReturnViewWithEditModel()
        {
            // Arrange
            var taskItemId = 1;

            var updateDto = new UpdateTaskItemDto
            {
                Id = taskItemId,
                Title = "Edit Task",
                Description = "Edit Description",
                Difficulty = 0.6,
                RequiredSkills = "C#, Angular",
                DeadlineDays = 7,
                EstimatedHours = 15,
                Status = TaskState.InProgress,
                TeamId = 2
            };

            var teamsData = new List<Team>
        {
            new Team { Id = 1, Name = "Development Team" },
            new Team { Id = 2, Name = "QA Team" }
        };

            var editModel = new TaskItemEditModel
            {
                TaskItem = updateDto,
                Teams = new SelectList(teamsData, "Id", "Name"),
                TaskStates = new List<SelectListItem>()
            };

            _mockTaskItemService
                .Setup(s => s.GetEditModelAsync(taskItemId, null))
                .ReturnsAsync(editModel);

            // Act
            var result = await _controller.Edit(taskItemId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TaskItemEditModel>().Subject;
            model.TaskItem.Id.Should().Be(taskItemId);
            model.TaskItem.Title.Should().Be("Edit Task");
            model.Teams.Should().HaveCount(2);

            _mockTaskItemService.Verify(s => s.GetEditModelAsync(taskItemId, null), Times.Once);
        }

        [Fact]
        public async Task Edit_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.GetEditModelAsync(It.IsAny<int>(), null), Times.Never);
        }

        [Fact]
        public async Task Edit_GET_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var taskItemId = 999;
            _mockTaskItemService
                .Setup(s => s.GetEditModelAsync(taskItemId, null))
                .ReturnsAsync((TaskItemEditModel?)null);

            // Act
            var result = await _controller.Edit(taskItemId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.GetEditModelAsync(taskItemId, null), Times.Once);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_POST_WithValidModel_ShouldUpdateTaskItemAndRedirectToIndex()
        {
            // Arrange
            var taskItemId = 1;
            var updateDto = new UpdateTaskItemDto
            {
                Id = taskItemId,
                Title = "Updated Task",
                Description = "Updated Description",
                Difficulty = 0.8,
                RequiredSkills = "C#, Vue",
                DeadlineDays = 10,
                EstimatedHours = 20,
                Status = TaskState.Done,
                TeamId = 2
            };

            var model = new TaskItemEditModel
            {
                TaskItem = updateDto
            };

            _mockTaskItemService
                .Setup(s => s.UpdateAsync(updateDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(taskItemId, model);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockTaskItemService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        [Fact]
        public async Task Edit_POST_WithMismatchedId_ShouldReturnNotFound()
        {
            // Arrange
            var routeId = 1;
            var modelId = 2;
            var updateDto = new UpdateTaskItemDto
            {
                Id = modelId,
                Title = "Test Task"
            };

            var model = new TaskItemEditModel
            {
                TaskItem = updateDto
            };

            // Act
            var result = await _controller.Edit(routeId, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.UpdateAsync(It.IsAny<UpdateTaskItemDto>()), Times.Never);
        }

        [Fact]
        public async Task Edit_POST_WithInvalidModel_ShouldReturnViewWithModelAndDropdowns()
        {
            // Arrange
            var taskItemId = 1;
            var updateDto = new UpdateTaskItemDto
            {
                Id = taskItemId,
                Title = "", // Invalid - empty title
                TeamId = 1
            };

            var model = new TaskItemEditModel
            {
                TaskItem = updateDto
            };

            var teamsData = new List<Team>
        {
            new Team { Id = 1, Name = "Development Team" }
        };

            var editModelWithDropdowns = new TaskItemEditModel
            {
                TaskItem = updateDto,
                Teams = new SelectList(teamsData, "Id", "Name"),
                TaskStates = new List<SelectListItem>()
            };

            _mockTaskItemService
                .Setup(s => s.GetEditModelAsync(taskItemId, updateDto))
                .ReturnsAsync(editModelWithDropdowns);

            _controller.ModelState.AddModelError("TaskItem.Title", "Title is required");

            // Act
            var result = await _controller.Edit(taskItemId, model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var resultModel = viewResult.Model.Should().BeAssignableTo<TaskItemEditModel>().Subject;
            resultModel.TaskItem.Should().BeEquivalentTo(updateDto);
            resultModel.Teams.Should().HaveCount(1);

            _mockTaskItemService.Verify(s => s.UpdateAsync(It.IsAny<UpdateTaskItemDto>()), Times.Never);
            _mockTaskItemService.Verify(s => s.GetEditModelAsync(taskItemId, updateDto), Times.Once);
        }

        [Fact]
        public async Task Edit_POST_WhenTaskItemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var taskItemId = 1;
            var updateDto = new UpdateTaskItemDto
            {
                Id = taskItemId,
                Title = "Updated Task"
            };

            var model = new TaskItemEditModel
            {
                TaskItem = updateDto
            };

            _mockTaskItemService
                .Setup(s => s.UpdateAsync(updateDto))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Edit(taskItemId, model);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task Delete_GET_WithValidId_ShouldReturnViewWithTaskItem()
        {
            // Arrange
            var taskItemId = 1;
            var taskItem = new TaskItem
            {
                Id = taskItemId,
                Title = "Task to Delete",
                Description = "Description of task to delete",
                TeamId = 1
            };

            _mockTaskItemService
                .Setup(s => s.GetByIdAsync(taskItemId))
                .ReturnsAsync(taskItem);

            // Act
            var result = await _controller.Delete(taskItemId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TaskItem>().Subject;
            model.Id.Should().Be(taskItemId);
            model.Title.Should().Be("Task to Delete");

            _mockTaskItemService.Verify(s => s.GetByIdAsync(taskItemId), Times.Once);
        }

        [Fact]
        public async Task Delete_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_GET_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var taskItemId = 999;
            _mockTaskItemService
                .Setup(s => s.GetByIdAsync(taskItemId))
                .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _controller.Delete(taskItemId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.GetByIdAsync(taskItemId), Times.Once);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ShouldDeleteTaskItemAndRedirectToIndex()
        {
            // Arrange
            var taskItemId = 1;
            _mockTaskItemService
                .Setup(s => s.DeleteAsync(taskItemId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(taskItemId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockTaskItemService.Verify(s => s.DeleteAsync(taskItemId), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenTaskItemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var taskItemId = 999;
            _mockTaskItemService
                .Setup(s => s.DeleteAsync(taskItemId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.DeleteConfirmed(taskItemId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTaskItemService.Verify(s => s.DeleteAsync(taskItemId), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenServiceThrowsOtherException_ShouldPropagateException()
        {
            // Arrange
            var taskItemId = 1;
            _mockTaskItemService
                .Setup(s => s.DeleteAsync(taskItemId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete task item with active assignments"));

            // Act & Assert
            var act = async () => await _controller.DeleteConfirmed(taskItemId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete task item with active assignments");
        }

        #endregion
    }
}
