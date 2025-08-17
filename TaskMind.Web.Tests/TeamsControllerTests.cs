using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Moq;
using TaskMind.Application.DTOs;
using TaskMind.Application.DTOs.Team;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Controllers;
using TaskMind.Domain.Models;

namespace TaskMind.Web.Tests
{
    public class TeamsControllerTests
    {
        private readonly Mock<ITeamService> _mockTeamService;
        private readonly TeamsController _controller;

        public TeamsControllerTests()
        {
            _mockTeamService = new Mock<ITeamService>();
            _controller = new TeamsController(_mockTeamService.Object);

            var mockValidator = new Mock<IObjectModelValidator>();
            _controller.ObjectValidator = mockValidator.Object;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ShouldReturnViewWithTeams()
        {
            // Arrange
            var teams = new List<Team>
        {
            new Team { Id = 1, Name = "Development Team" },
            new Team { Id = 2, Name = "QA Team" }
        };

            _mockTeamService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(teams);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<Team>>().Subject;
            model.Should().HaveCount(2);
            model.Should().Contain(t => t.Name == "Development Team");
            model.Should().Contain(t => t.Name == "QA Team");

            _mockTeamService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Index_WhenNoTeams_ShouldReturnViewWithEmptyList()
        {
            // Arrange
            _mockTeamService
                .Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<Team>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<Team>>().Subject;
            model.Should().BeEmpty();
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockTeamService
                .Setup(s => s.GetAllAsync())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            var act = async () => await _controller.Index();
            await act.Should().ThrowAsync<Exception>().WithMessage("Service error");
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_WithValidId_ShouldReturnViewWithTeam()
        {
            // Arrange
            var teamId = 1;
            var team = new Team
            {
                Id = teamId,
                Name = "Development Team",
                Employees = new List<Employee>
            {
                new Employee { Id = 1, Name = "John Doe" }
            }
            };

            _mockTeamService
                .Setup(s => s.GetByIdAsync(teamId))
                .ReturnsAsync(team);

            // Act
            var result = await _controller.Details(teamId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Team>().Subject;
            model.Id.Should().Be(teamId);
            model.Name.Should().Be("Development Team");
            model.Employees.Should().HaveCount(1);

            _mockTeamService.Verify(s => s.GetByIdAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task Details_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Details_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 999;
            _mockTeamService
                .Setup(s => s.GetByIdAsync(teamId))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _controller.Details(teamId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetByIdAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task Details_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var teamId = 1;
            _mockTeamService
                .Setup(s => s.GetByIdAsync(teamId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var act = async () => await _controller.Details(teamId);
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_GET_ShouldReturnView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_POST_WithValidModel_ShouldCreateTeamAndRedirectToIndex()
        {
            // Arrange
            var createDto = new CreateTeamDto
            {
                Name = "New Team"
            };

            _mockTeamService
                .Setup(s => s.CreateAsync(createDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockTeamService.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_POST_WithInvalidModel_ShouldReturnViewWithModel()
        {
            // Arrange
            var createDto = new CreateTeamDto
            {
                Name = "" // Invalid - empty name
            };

            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(createDto);

            _mockTeamService.Verify(s => s.CreateAsync(It.IsAny<CreateTeamDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_POST_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var createDto = new CreateTeamDto
            {
                Name = "Test Team"
            };

            _mockTeamService
                .Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new Exception("Creation failed"));

            // Act & Assert
            var act = async () => await _controller.Create(createDto);
            await act.Should().ThrowAsync<Exception>().WithMessage("Creation failed");
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_GET_WithValidId_ShouldReturnViewWithEditModel()
        {
            // Arrange
            var teamId = 1;
            var editModel = new UpdateTeamDto
            {
                Id = teamId,
                Name = "Development Team"
            };

            _mockTeamService
                .Setup(s => s.GetForEditAsync(teamId))
                .ReturnsAsync(editModel);

            // Act
            var result = await _controller.Edit(teamId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<UpdateTeamDto>().Subject;
            model.Id.Should().Be(teamId);
            model.Name.Should().Be("Development Team");

            _mockTeamService.Verify(s => s.GetForEditAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task Edit_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetForEditAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Edit_GET_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 999;
            _mockTeamService
                .Setup(s => s.GetForEditAsync(teamId))
                .ReturnsAsync((UpdateTeamDto?)null);

            // Act
            var result = await _controller.Edit(teamId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetForEditAsync(teamId), Times.Once);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_POST_WithValidModel_ShouldUpdateTeamAndRedirectToIndex()
        {
            // Arrange
            var teamId = 1;
            var updateDto = new UpdateTeamDto
            {
                Id = teamId,
                Name = "Updated Team Name"
            };

            _mockTeamService
                .Setup(s => s.UpdateAsync(updateDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(teamId, updateDto);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockTeamService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        [Fact]
        public async Task Edit_POST_WithMismatchedId_ShouldReturnNotFound()
        {
            // Arrange
            var routeId = 1;
            var modelId = 2;
            var updateDto = new UpdateTeamDto
            {
                Id = modelId,
                Name = "Test Team"
            };

            // Act
            var result = await _controller.Edit(routeId, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.UpdateAsync(It.IsAny<UpdateTeamDto>()), Times.Never);
        }

        [Fact]
        public async Task Edit_POST_WithInvalidModel_ShouldReturnViewWithModel()
        {
            // Arrange
            var teamId = 1;
            var updateDto = new UpdateTeamDto
            {
                Id = teamId,
                Name = "" // Invalid - empty name
            };

            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Edit(teamId, updateDto);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(updateDto);

            _mockTeamService.Verify(s => s.UpdateAsync(It.IsAny<UpdateTeamDto>()), Times.Never);
        }

        [Fact]
        public async Task Edit_POST_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 1;
            var updateDto = new UpdateTeamDto
            {
                Id = teamId,
                Name = "Updated Team"
            };

            _mockTeamService
                .Setup(s => s.UpdateAsync(updateDto))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Edit(teamId, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.UpdateAsync(updateDto), Times.Once);
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task Delete_GET_WithValidId_ShouldReturnViewWithTeam()
        {
            // Arrange
            var teamId = 1;
            var team = new Team
            {
                Id = teamId,
                Name = "Team to Delete"
            };

            _mockTeamService
                .Setup(s => s.GetByIdAsync(teamId))
                .ReturnsAsync(team);

            // Act
            var result = await _controller.Delete(teamId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Team>().Subject;
            model.Id.Should().Be(teamId);
            model.Name.Should().Be("Team to Delete");

            _mockTeamService.Verify(s => s.GetByIdAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task Delete_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_GET_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 999;
            _mockTeamService
                .Setup(s => s.GetByIdAsync(teamId))
                .ReturnsAsync((Team?)null);

            // Act
            var result = await _controller.Delete(teamId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetByIdAsync(teamId), Times.Once);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ShouldDeleteTeamAndRedirectToIndex()
        {
            // Arrange
            var teamId = 1;
            _mockTeamService
                .Setup(s => s.DeleteAsync(teamId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(teamId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockTeamService.Verify(s => s.DeleteAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 999;
            _mockTeamService
                .Setup(s => s.DeleteAsync(teamId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.DeleteConfirmed(teamId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.DeleteAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenServiceThrowsOtherException_ShouldPropagateException()
        {
            // Arrange
            var teamId = 1;
            _mockTeamService
                .Setup(s => s.DeleteAsync(teamId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete team with active employees"));

            // Act & Assert
            var act = async () => await _controller.DeleteConfirmed(teamId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete team with active employees");
        }

        #endregion

        #region ManageTasks Tests

        [Fact]
        public async Task ManageTasks_WithValidId_ShouldReturnViewWithTeamTasksModel()
        {
            // Arrange
            var teamId = 1;
            var teamTasksModel = new TeamTasksModel
            {
                Team = new Team { Id = teamId, Name = "Development Team" },
                Tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1", TeamId = teamId },
                new TaskItem { Id = 2, Title = "Task 2", TeamId = teamId }
            }
            };

            _mockTeamService
                .Setup(s => s.GetTeamTasksAsync(teamId))
                .ReturnsAsync(teamTasksModel);

            // Act
            var result = await _controller.ManageTasks(teamId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TeamTasksModel>().Subject;
            model.Team.Id.Should().Be(teamId);
            model.Team.Name.Should().Be("Development Team");
            model.Tasks.Should().HaveCount(2);

            _mockTeamService.Verify(s => s.GetTeamTasksAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task ManageTasks_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.ManageTasks(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetTeamTasksAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ManageTasks_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 999;
            _mockTeamService
                .Setup(s => s.GetTeamTasksAsync(teamId))
                .ReturnsAsync((TeamTasksModel?)null);

            // Act
            var result = await _controller.ManageTasks(teamId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockTeamService.Verify(s => s.GetTeamTasksAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task ManageTasks_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var teamId = 1;
            _mockTeamService
                .Setup(s => s.GetTeamTasksAsync(teamId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var act = async () => await _controller.ManageTasks(teamId);
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }

        #endregion
    }
}
