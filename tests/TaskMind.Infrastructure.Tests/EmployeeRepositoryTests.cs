using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskMind.Domain.Models;
using TaskMind.Infrastructure.Data;
using TaskMind.Infrastructure.Repositories;

namespace TaskMind.Infrastructure.Tests
{
    public class EmployeeRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeeRepository _repository;

        public EmployeeRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new EmployeeRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldAddEmployeeToDatabase()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "John Doe",
                Skills = new List<string> { "C#", "React" },
                CurrentWorkload = 0.5,
                TaskCompletionSpeed = 8.0,
                TeamId = 1
            };

            // Act
            var result = await _repository.CreateAsync(employee);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("John Doe");

            var employeeInDb = await _context.Employees.FindAsync(result.Id);
            employeeInDb.Should().NotBeNull();
            employeeInDb!.Name.Should().Be("John Doe");
            employeeInDb.Skills.Should().BeEquivalentTo(new List<string> { "C#", "React" });
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnEmployeeWithGeneratedId()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Jane Smith",
                Skills = new List<string> { "Python" },
                CurrentWorkload = 0.3,
                TaskCompletionSpeed = 7.5,
                TeamId = 2
            };

            // Act
            var result = await _repository.CreateAsync(employee);

            // Assert
            result.Id.Should().BeGreaterThan(0);
            employee.Id.Should().Be(result.Id); // Original object should be updated
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenNoEmployees_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEmployeesWithTeams()
        {
            // Arrange
            var team1 = new Team { Name = "Frontend Team" };
            var team2 = new Team { Name = "Backend Team" };
            await _context.Teams.AddRangeAsync(team1, team2);
            await _context.SaveChangesAsync();

            var employee1 = new Employee
            {
                Name = "John Doe",
                Skills = new List<string> { "React" },
                CurrentWorkload = 0.5,
                TaskCompletionSpeed = 8.0,
                TeamId = team1.Id
            };
            var employee2 = new Employee
            {
                Name = "Jane Smith",
                Skills = new List<string> { "C#" },
                CurrentWorkload = 0.7,
                TaskCompletionSpeed = 7.0,
                TeamId = team2.Id
            };

            await _context.Employees.AddRangeAsync(employee1, employee2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);

            var johnDoe = result.First(e => e.Name == "John Doe");
            johnDoe.Team.Should().NotBeNull();
            johnDoe.Team!.Name.Should().Be("Frontend Team");

            var janeSmith = result.First(e => e.Name == "Jane Smith");
            janeSmith.Team.Should().NotBeNull();
            janeSmith.Team!.Name.Should().Be("Backend Team");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenEmployeeExists_ShouldReturnEmployeeWithTeam()
        {
            // Arrange
            var team = new Team { Name = "Development Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                Name = "Test Employee",
                Skills = new List<string> { "C#", "SQL" },
                CurrentWorkload = 0.6,
                TaskCompletionSpeed = 8.5,
                TeamId = team.Id
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(employee.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Employee");
            result.Skills.Should().BeEquivalentTo(new List<string> { "C#", "SQL" });
            result.CurrentWorkload.Should().Be(0.6);
            result.TaskCompletionSpeed.Should().Be(8.5);
            result.Team.Should().NotBeNull();
            result.Team!.Name.Should().Be("Development Team");
        }

        [Fact]
        public async Task GetByIdAsync_WhenEmployeeNotExists_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByTeamIdAsync Tests

        [Fact]
        public async Task GetByTeamIdAsync_ShouldReturnEmployeesFromSpecificTeam()
        {
            // Arrange
            var team1 = new Team { Name = "Team 1" };
            var team2 = new Team { Name = "Team 2" };
            await _context.Teams.AddRangeAsync(team1, team2);
            await _context.SaveChangesAsync();

            var employee1 = new Employee
            {
                Name = "Employee 1",
                Skills = new List<string> { "C#" },
                TeamId = team1.Id
            };
            var employee2 = new Employee
            {
                Name = "Employee 2",
                Skills = new List<string> { "React" },
                TeamId = team1.Id
            };
            var employee3 = new Employee
            {
                Name = "Employee 3",
                Skills = new List<string> { "Python" },
                TeamId = team2.Id
            };

            await _context.Employees.AddRangeAsync(employee1, employee2, employee3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByTeamIdAsync(team1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.TeamId == team1.Id);
            result.Should().Contain(e => e.Name == "Employee 1");
            result.Should().Contain(e => e.Name == "Employee 2");
            result.Should().NotContain(e => e.Name == "Employee 3");

            // Verify Team navigation property is loaded
            result.Should().OnlyContain(e => e.Team != null);
            result.Should().OnlyContain(e => e.Team!.Name == "Team 1");
        }

        [Fact]
        public async Task GetByTeamIdAsync_WhenNoEmployeesInTeam_ShouldReturnEmptyList()
        {
            // Arrange
            var team = new Team { Name = "Empty Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByTeamIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByTeamIdAsync_WhenTeamNotExists_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetByTeamIdAsync(999);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenEmployeeExists_ShouldReturnTrue()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Existing Employee",
                Skills = new List<string> { "C#" },
                TeamId = 1
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(employee.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenEmployeeNotExists_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEmployeeInDatabase()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Original Name",
                Skills = new List<string> { "C#" },
                CurrentWorkload = 0.5,
                TaskCompletionSpeed = 7.0,
                TeamId = 1
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            // Modify employee
            employee.Name = "Updated Name";
            employee.Skills = new List<string> { "C#", "React", "SQL" };
            employee.CurrentWorkload = 0.8;
            employee.TaskCompletionSpeed = 9.0;
            employee.TeamId = 2;

            // Act
            var result = await _repository.UpdateAsync(employee.Id, employee);

            // Assert
            result.Should().Be(5);

            // Verify in database
            var employeeInDb = await _context.Employees.FindAsync(employee.Id);
            employeeInDb.Should().NotBeNull();
            employeeInDb!.Name.Should().Be("Updated Name");
            employeeInDb.Skills.Should().BeEquivalentTo(new[] { "C#", "React", "SQL" });
            employeeInDb.CurrentWorkload.Should().Be(0.8);
            employeeInDb.TaskCompletionSpeed.Should().Be(9.0);
            employeeInDb.TeamId.Should().Be(2);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedEmployeeRows()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test Employee",
                Skills = new List<string> { "Python" },
                CurrentWorkload = 0.3,
                TaskCompletionSpeed = 6.0,
                TeamId = 1
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            employee.Name = "Modified Employee";

            // Act
            var result = await _repository.UpdateAsync(employee.Id, employee);

            // Assert
            result.Should().Be(1);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenEmployeeExists_ShouldRemoveFromDatabase()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Employee to Delete",
                Skills = new List<string> { "Java" },
                TeamId = 1
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var employeeId = employee.Id;

            // Act
            await _repository.DeleteAsync(employeeId);

            // Assert
            var employeeInDb = await _context.Employees.FindAsync(employeeId);
            employeeInDb.Should().BeNull();

            var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenEmployeeNotExists_ShouldNotThrowException()
        {
            // Act & Assert
            var act = async () => await _repository.DeleteAsync(999);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteAsync_WhenEmployeeNotExists_ShouldNotAffectOtherEmployees()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Existing Employee",
                Skills = new List<string> { "C#" },
                TeamId = 1
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var originalCount = await _context.Employees.CountAsync();

            // Act
            await _repository.DeleteAsync(999);

            // Assert
            var newCount = await _context.Employees.CountAsync();
            newCount.Should().Be(originalCount);

            var existingEmployee = await _context.Employees.FindAsync(employee.Id);
            existingEmployee.Should().NotBeNull();
        }

        #endregion
    }
}
