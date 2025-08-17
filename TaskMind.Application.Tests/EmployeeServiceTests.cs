using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using TaskMind.Application.DTOs.Employee;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Tests
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
        private readonly Mock<ITeamRepository> _mockTeamRepo;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _mockEmployeeRepo = new Mock<IEmployeeRepository>();
            _mockTeamRepo = new Mock<ITeamRepository>();
            _employeeService = new EmployeeService(_mockEmployeeRepo.Object, _mockTeamRepo.Object);
        }

        #region GetAllEmployeesAsync Tests

        [Fact]
        public async Task GetAllEmployeesAsync_WhenCalled_ShouldReturnAllEmployees()
        {
            // Arrange
            var expectedEmployees = new List<Employee>
        {
            new() { Id = 1, Name = "John Doe", Skills = new List<string> { "C#", "React" } },
            new() { Id = 2, Name = "Jane Smith", Skills = new List<string> { "Python", "SQL" } }
        };
            _mockEmployeeRepo.Setup(x => x.GetAllAsync())
                            .ReturnsAsync(expectedEmployees);

            // Act
            var result = await _employeeService.GetAllEmployeesAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedEmployees);
            _mockEmployeeRepo.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_WhenNoEmployees_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockEmployeeRepo.Setup(x => x.GetAllAsync())
                            .ReturnsAsync(new List<Employee>());

            // Act
            var result = await _employeeService.GetAllEmployeesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetEmployeeByIdAsync Tests

        [Fact]
        public async Task GetEmployeeByIdAsync_WhenEmployeeExists_ShouldReturnEmployee()
        {
            // Arrange
            var employeeId = 1;
            var expectedEmployee = new Employee
            {
                Id = employeeId,
                Name = "John Doe",
                Skills = new List<string> { "C#" }
            };
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(expectedEmployee);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

            // Assert
            result.Should().BeEquivalentTo(expectedEmployee);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WhenEmployeeNotExists_ShouldReturnNull()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync((Employee?)null);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(employeeId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetTeamsForDropdownAsync Tests

        [Fact]
        public async Task GetTeamsForDropdownAsync_ShouldReturnSelectListWithTeams()
        {
            // Arrange
            var teams = new List<Team>
        {
            new() { Id = 1, Name = "Frontend Team" },
            new() { Id = 2, Name = "Backend Team" }
        };
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _employeeService.GetTeamsForDropdownAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<SelectList>();
            var items = result.Items.Cast<Team>().ToList();
            items.Should().HaveCount(2);
            items.Should().Contain(t => t.Name == "Frontend Team");
            items.Should().Contain(t => t.Name == "Backend Team");
        }

        [Fact]
        public async Task GetTeamsForDropdownAsync_WithSelectedTeamId_ShouldSetSelectedValue()
        {
            // Arrange
            var selectedTeamId = 2;
            var teams = new List<Team>
        {
            new() { Id = 1, Name = "Frontend Team" },
            new() { Id = 2, Name = "Backend Team" }
        };
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _employeeService.GetTeamsForDropdownAsync(selectedTeamId);

            // Assert
            result.SelectedValue.Should().Be(selectedTeamId);
        }

        #endregion

        #region GetEmployeeForEditAsync Tests

        [Fact]
        public async Task GetEmployeeForEditAsync_WhenEmployeeExists_ShouldReturnEmployeeEditModel()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John Doe",
                Skills = new List<string> { "C#", "React" },
                CurrentWorkload = 75,
                TaskCompletionSpeed = 8.5,
                TeamId = 2
            };
            var teams = new List<Team>
        {
            new() { Id = 1, Name = "Frontend Team" },
            new() { Id = 2, Name = "Backend Team" }
        };

            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(employee);
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(teams);

            // Act
            var result = await _employeeService.GetEmployeeForEditAsync(employeeId);

            // Assert
            result.Should().NotBeNull();
            result!.Employee.Should().NotBeNull();
            result.Employee.Id.Should().Be(employeeId);
            result.Employee.Name.Should().Be("John Doe");
            result.Employee.Skills.Should().Be("C#, React");
            result.Employee.CurrentWorkload.Should().Be(75);
            result.Employee.TaskCompletionSpeed.Should().Be(8.5);
            result.Employee.TeamId.Should().Be(2);
            result.Teams.Should().NotBeNull();
        }

        [Fact]
        public async Task GetEmployeeForEditAsync_WhenEmployeeNotExists_ShouldReturnNull()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync((Employee?)null);

            // Act
            var result = await _employeeService.GetEmployeeForEditAsync(employeeId);

            // Assert
            result.Should().BeNull();
            _mockTeamRepo.Verify(x => x.GetAllAsync(), Times.Never);
        }

        #endregion

        #region CreateEmployeeAsync Tests

        [Fact]
        public async Task CreateEmployeeAsync_ShouldCallRepositoryCreate()
        {
            // Arrange
            var createDto = new CreateEmployeeDto
            {
                Name = "New Employee",
                Skills = "C#, SQL",
                CurrentWorkload = 50,
                TaskCompletionSpeed = 7.0,
                TeamId = 1
            };

            // Act
            await _employeeService.CreateEmployeeAsync(createDto);

            // Assert
            _mockEmployeeRepo.Verify(x => x.CreateAsync(It.IsAny<Employee>()), Times.Once);
        }

        #endregion

        #region UpdateEmployeeAsync Tests

        [Fact]
        public async Task UpdateEmployeeAsync_WhenEmployeeExists_ShouldUpdateEmployee()
        {
            // Arrange
            var updateDto = new UpdateEmployeeDto
            {
                Id = 1,
                Name = "Updated Name",
                Skills = "C#, React, SQL",
                CurrentWorkload = 80,
                TaskCompletionSpeed = 9.0,
                TeamId = 2
            };
            var existingEmployee = new Employee
            {
                Id = 1,
                Name = "Old Name",
                Skills = new List<string> { "C#" },
                CurrentWorkload = 50,
                TaskCompletionSpeed = 7.0,
                TeamId = 1
            };

            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(updateDto.Id))
                            .ReturnsAsync(existingEmployee);

            // Act
            await _employeeService.UpdateEmployeeAsync(updateDto);

            // Assert
            _mockEmployeeRepo.Verify(x => x.GetByIdAsync(updateDto.Id), Times.Once);
            _mockEmployeeRepo.Verify(x => x.UpdateAsync(existingEmployee), Times.Once);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WhenEmployeeNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateEmployeeDto { Id = 999 };
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(updateDto.Id))
                            .ReturnsAsync((Employee?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _employeeService.UpdateEmployeeAsync(updateDto));

            exception.Message.Should().Be("Employee not found");
            _mockEmployeeRepo.Verify(x => x.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        }

        #endregion

        #region DeleteEmployeeAsync Tests

        [Fact]
        public async Task DeleteEmployeeAsync_WhenEmployeeExists_ShouldDeleteEmployee()
        {
            // Arrange
            var employeeId = 1;
            var existingEmployee = new Employee { Id = employeeId, Name = "John Doe" };
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync(existingEmployee);

            // Act
            await _employeeService.DeleteEmployeeAsync(employeeId);

            // Assert
            _mockEmployeeRepo.Verify(x => x.GetByIdAsync(employeeId), Times.Once);
            _mockEmployeeRepo.Verify(x => x.DeleteAsync(employeeId), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_WhenEmployeeNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeRepo.Setup(x => x.GetByIdAsync(employeeId))
                            .ReturnsAsync((Employee?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _employeeService.DeleteEmployeeAsync(employeeId));

            exception.Message.Should().Be("Employee not found");
            _mockEmployeeRepo.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenEmployeeExists_ShouldReturnTrue()
        {
            // Arrange
            var employeeId = 1;
            _mockEmployeeRepo.Setup(x => x.ExistsAsync(employeeId))
                            .ReturnsAsync(true);

            // Act
            var result = await _employeeService.ExistsAsync(employeeId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenEmployeeNotExists_ShouldReturnFalse()
        {
            // Arrange
            var employeeId = 999;
            _mockEmployeeRepo.Setup(x => x.ExistsAsync(employeeId))
                            .ReturnsAsync(false);

            // Act
            var result = await _employeeService.ExistsAsync(employeeId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
