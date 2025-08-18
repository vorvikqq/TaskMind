using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Application.Services.Interfaces;
using TaskMind.Controllers;
using TaskMind.Domain.Models;

namespace TaskMind.Web.Tests
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeService> _mockEmployeeService;
        private readonly EmployeesController _controller;

        public EmployeesControllerTests()
        {
            _mockEmployeeService = new Mock<IEmployeeService>();
            _controller = new EmployeesController(_mockEmployeeService.Object);

            var mockValidator = new Mock<IObjectModelValidator>();
            _controller.ObjectValidator = mockValidator.Object;
        }

        #region Index Tests

        [Fact]
        public async Task Index_ShouldReturnViewWithEmployees()
        {
            // Arrange
            var employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "John Doe", TeamId = 1 },
            new Employee { Id = 2, Name = "Jane Smith", TeamId = 2 }
        };

            _mockEmployeeService
                .Setup(s => s.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<Employee>>().Subject;
            model.Should().HaveCount(2);
            model.Should().Contain(e => e.Name == "John Doe");
            model.Should().Contain(e => e.Name == "Jane Smith");

            _mockEmployeeService.Verify(s => s.GetAllEmployeesAsync(), Times.Once);
        }

        [Fact]
        public async Task Index_WhenNoEmployees_ShouldReturnViewWithEmptyList()
        {
            // Arrange
            _mockEmployeeService
                .Setup(s => s.GetAllEmployeesAsync())
                .ReturnsAsync(new List<Employee>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<Employee>>().Subject;
            model.Should().BeEmpty();
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            _mockEmployeeService
                .Setup(s => s.GetAllEmployeesAsync())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            var act = async () => await _controller.Index();
            await act.Should().ThrowAsync<Exception>().WithMessage("Service error");
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_WithValidId_ShouldReturnViewWithEmployee()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John Doe",
                TeamId = 1,
                Skills = new List<string> { "C#", "React" }
            };

            _mockEmployeeService
                .Setup(s => s.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.Details(employeeId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Employee>().Subject;
            model.Id.Should().Be(employeeId);
            model.Name.Should().Be("John Doe");
            model.TeamId.Should().Be(1);

            _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task Details_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Details_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeService
                .Setup(s => s.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act
            var result = await _controller.Details(employeeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task Details_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var employeeId = 1;
            _mockEmployeeService
                .Setup(s => s.GetEmployeeByIdAsync(employeeId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var act = async () => await _controller.Details(employeeId);
            await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public async Task Create_GET_ShouldReturnViewWithTeamsInViewData()
        {
            // Arrange
            var teamsData = new List<Team>
            {
                new Team { Id = 1, Name = "Development Team" },
                new Team { Id = 2, Name = "QA Team" }
            };

            var selectList = new SelectList(teamsData, "Id", "Name");

            _mockEmployeeService
                .Setup(s => s.GetTeamsForDropdownAsync(It.IsAny<int?>()))
                .ReturnsAsync(selectList); ;

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData["Team"].Should().BeEquivalentTo(selectList);

            _mockEmployeeService.Verify(s => s.GetTeamsForDropdownAsync(It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task Create_GET_WhenNoTeams_ShouldReturnViewWithEmptyTeamsDropdown()
        {
            // Arrange
            _mockEmployeeService
                .Setup(s => s.GetTeamsForDropdownAsync(It.IsAny<int?>()))
                .ReturnsAsync(new SelectList(Enumerable.Empty<SelectListItem>()));

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var teams = viewResult.ViewData["Team"].Should().BeAssignableTo<SelectList>().Subject;
            teams.Should().BeEmpty();
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_POST_WithValidModel_ShouldCreateEmployeeAndRedirectToIndex()
        {
            // Arrange
            var createDto = new CreateEmployeeRequest
            {
                Name = "New Employee",
                Skills = "C#, React",
                TeamId = 1
            };

            _mockEmployeeService
                .Setup(s => s.CreateEmployeeAsync(createDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockEmployeeService.Verify(s => s.CreateEmployeeAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_POST_WithInvalidModel_ShouldReturnViewWithModelAndTeams()
        {
            // Arrange
            var createDto = new CreateEmployeeRequest
            {
                Name = "", // Invalid - empty name
                TeamId = 1
            };

            var teamsData = new List<Team>
            {
                new Team { Id = 1, Name = "Development Team" },
                new Team { Id = 2, Name = "QA Team" }
            };

            var selectList = new SelectList(teamsData, "Id", "Name");

            _mockEmployeeService
                .Setup(s => s.GetTeamsForDropdownAsync(It.IsAny<int?>()))
                .ReturnsAsync(selectList); ;


            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(createDto);
            viewResult.ViewData["Team"].Should().BeEquivalentTo(selectList);

            _mockEmployeeService.Verify(s => s.CreateEmployeeAsync(It.IsAny<CreateEmployeeRequest>()), Times.Never);
            _mockEmployeeService.Verify(s => s.GetTeamsForDropdownAsync(createDto.TeamId), Times.Once);
        }

        [Fact]
        public async Task Create_POST_WhenServiceThrowsException_ShouldPropagateException()
        {
            // Arrange
            var createDto = new CreateEmployeeRequest
            {
                Name = "Test Employee",
                TeamId = 1
            };

            _mockEmployeeService
                .Setup(s => s.CreateEmployeeAsync(createDto))
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
            var employeeId = 1;

            var teamsData = new List<Team>
            {
                new Team { Id = 1, Name = "Development Team" },
                new Team { Id = 2, Name = "QA Team" }
            };

            var selectList = new SelectList(teamsData, "Id", "Name");


            var editModel = new EditEmployeeResponse
            {
                Employee = new UpdateEmployeeRequest { Id = employeeId, Name = "John Doe", TeamId = 1 },
                Teams = selectList
            };

            _mockEmployeeService
                .Setup(s => s.GetEmployeeForEditAsync(employeeId))
                .ReturnsAsync(editModel);

            // Act
            var result = await _controller.Edit(employeeId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<EditEmployeeResponse>().Subject;
            model.Employee.Id.Should().Be(employeeId);
            model.Employee.Name.Should().Be("John Doe");
            model.Teams.Should().HaveCount(2);

            _mockEmployeeService.Verify(s => s.GetEmployeeForEditAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task Edit_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.GetEmployeeForEditAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Edit_GET_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeService
                .Setup(s => s.GetEmployeeForEditAsync(employeeId))
                .ReturnsAsync((EditEmployeeResponse?)null);

            // Act
            var result = await _controller.Edit(employeeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.GetEmployeeForEditAsync(employeeId), Times.Once);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_POST_WithValidModel_ShouldUpdateEmployeeAndRedirectToIndex()
        {
            // Arrange
            var employeeId = 1;
            var editModel = new EditEmployeeResponse
            {
                Employee = new UpdateEmployeeRequest
                {
                    Id = employeeId,
                    Name = "Updated Employee",
                    CurrentWorkload = 0.1,
                    TaskCompletionSpeed = 0.1,
                    Skills = "C#,Angular",
                    TeamId = 2
                }
            };

            _mockEmployeeService
                .Setup(s => s.UpdateEmployeeAsync(editModel.Employee))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(employeeId, editModel);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockEmployeeService.Verify(s => s.UpdateEmployeeAsync(editModel.Employee), Times.Once);
        }

        [Fact]
        public async Task Edit_POST_WithMismatchedId_ShouldReturnNotFound()
        {
            // Arrange
            var routeId = 1;
            var modelId = 2;
            var editModel = new EditEmployeeResponse
            {
                Employee = new UpdateEmployeeRequest { Id = modelId, Name = "Test" }
            };

            // Act
            var result = await _controller.Edit(routeId, editModel);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.UpdateEmployeeAsync(It.IsAny<UpdateEmployeeRequest>()), Times.Never);
        }

        [Fact]
        public async Task Edit_POST_WhenEmployeeNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = 1;
            var editModel = new EditEmployeeResponse
            {
                Employee = new UpdateEmployeeRequest { Id = employeeId, Name = "Updated Employee" }
            };

            _mockEmployeeService
                .Setup(s => s.UpdateEmployeeAsync(editModel.Employee))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Edit(employeeId, editModel);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.UpdateEmployeeAsync(editModel.Employee), Times.Once);
        }

        #endregion

        #region Delete GET Tests

        [Fact]
        public async Task Delete_GET_WithValidId_ShouldReturnViewWithEmployee()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee
            {
                Id = employeeId,
                Name = "Employee to Delete",
                TeamId = 1
            };

            _mockEmployeeService
                .Setup(s => s.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.Delete(employeeId);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Employee>().Subject;
            model.Id.Should().Be(employeeId);
            model.Name.Should().Be("Employee to Delete");

            _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task Delete_GET_WithNullId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Delete_GET_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeService
                .Setup(s => s.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee?)null);

            // Act
            var result = await _controller.Delete(employeeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.GetEmployeeByIdAsync(employeeId), Times.Once);
        }

        #endregion

        #region Delete POST Tests

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ShouldDeleteEmployeeAndRedirectToIndex()
        {
            // Arrange
            var employeeId = 1;
            _mockEmployeeService
                .Setup(s => s.DeleteEmployeeAsync(employeeId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteConfirmed(employeeId);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            _mockEmployeeService.Verify(s => s.DeleteEmployeeAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenEmployeeNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeService
                .Setup(s => s.DeleteEmployeeAsync(employeeId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.DeleteConfirmed(employeeId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockEmployeeService.Verify(s => s.DeleteEmployeeAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenServiceThrowsOtherException_ShouldPropagateException()
        {
            // Arrange
            var employeeId = 1;
            _mockEmployeeService
                .Setup(s => s.DeleteEmployeeAsync(employeeId))
                .ThrowsAsync(new InvalidOperationException("Cannot delete employee with active tasks"));

            // Act & Assert
            var act = async () => await _controller.DeleteConfirmed(employeeId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete employee with active tasks");
        }

        #endregion
    }
}
