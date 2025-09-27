using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using TaskMind.Domain.Constants;
using TaskMind.Domain.Models;
using TaskMind.Infrastructure.Data;
using TaskMind.Infrastructure.Repositories;

namespace TaskMind.Infrastructure.Tests
{
    public class TaskItemRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskItemRepository _repository;
        private readonly DbConnection _connection;

        public TaskItemRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Database.EnsureCreated();

            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
            _repository = new TaskItemRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldAddTaskToDatabase()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Test Task",
                Description = "Task description",
                Difficulty = 5.0,
                RequiredSkills = new List<string> { "C#", "React" },
                DeadlineDays = 7,
                EstimatedHours = 40,
                Status = TaskState.New,
                TeamId = team.Id
            };

            // Act
            var result = await _repository.CreateAsync(task);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("Test Task");
            result.Description.Should().Be("Task description");
            result.Difficulty.Should().Be(5.0);
            result.RequiredSkills.Should().BeEquivalentTo(new[] { "C#", "React" });
            result.DeadlineDays.Should().Be(7);
            result.EstimatedHours.Should().Be(40);
            result.Status.Should().Be(TaskState.New);
            result.TeamId.Should().Be(team.Id);

            var taskInDb = await _context.Tasks.FindAsync(result.Id);
            taskInDb.Should().NotBeNull();
            taskInDb!.Title.Should().Be("Test Task");
            taskInDb.TeamId.Should().Be(team.Id);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnTaskWithGeneratedId()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Another Task",
                TeamId = team.Id,
                Status = TaskState.InProgress
            };

            // Act
            var result = await _repository.CreateAsync(task);

            // Assert
            result.Id.Should().BeGreaterThan(0);
            task.Id.Should().Be(result.Id); // Original object should be updated
        }

        [Fact]
        public async Task CreateAsync_WithEmployee_ShouldCreateSuccessfully()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                Name = "John Doe",
                Skills = new List<string> { "C#" },
                TeamId = team.Id
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Assigned Task",
                TeamId = team.Id,
                EmployeeId = employee.Id,
                Status = TaskState.InProgress
            };

            // Act
            var result = await _repository.CreateAsync(task);

            // Assert
            result.Should().NotBeNull();
            result.EmployeeId.Should().Be(employee.Id);
            result.TeamId.Should().Be(team.Id);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenNoTasks_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTasksWithTeamAndEmployee()
        {
            // Arrange
            var team1 = new Team { Name = "Team 1" };
            var team2 = new Team { Name = "Team 2" };
            await _context.Teams.AddRangeAsync(team1, team2);
            await _context.SaveChangesAsync();

            var employee1 = new Employee { Name = "John Doe", TeamId = team1.Id };
            var employee2 = new Employee { Name = "Jane Smith", TeamId = team2.Id };
            await _context.Employees.AddRangeAsync(employee1, employee2);
            await _context.SaveChangesAsync();

            var task1 = new TaskItem
            {
                Title = "Task 1",
                TeamId = team1.Id,
                EmployeeId = employee1.Id,
                Status = TaskState.InProgress
            };
            var task2 = new TaskItem
            {
                Title = "Task 2",
                TeamId = team2.Id,
                EmployeeId = employee2.Id,
                Status = TaskState.New
            };
            var task3 = new TaskItem
            {
                Title = "Task 3",
                TeamId = team1.Id,
                Status = TaskState.Done
            };

            await _context.Tasks.AddRangeAsync(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(3);

            var firstTask = result.First(t => t.Title == "Task 1");
            firstTask.Team.Should().NotBeNull();
            firstTask.Team!.Name.Should().Be("Team 1");
            firstTask.Employee.Should().NotBeNull();
            firstTask.Employee!.Name.Should().Be("John Doe");

            var secondTask = result.First(t => t.Title == "Task 2");
            secondTask.Team.Should().NotBeNull();
            secondTask.Team!.Name.Should().Be("Team 2");
            secondTask.Employee.Should().NotBeNull();
            secondTask.Employee!.Name.Should().Be("Jane Smith");

            var thirdTask = result.First(t => t.Title == "Task 3");
            thirdTask.Team.Should().NotBeNull();
            thirdTask.Team!.Name.Should().Be("Team 1");
            thirdTask.Employee.Should().BeNull();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTaskExists_ShouldReturnTaskWithTeamAndEmployee()
        {
            // Arrange
            var team = new Team { Name = "Development Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                Name = "Developer",
                Skills = new List<string> { "C#", "SQL" },
                TeamId = team.Id
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Complex Task",
                Description = "Detailed description",
                Difficulty = 7.5,
                RequiredSkills = new List<string> { "C#", "React", "SQL" },
                DeadlineDays = 14,
                EstimatedHours = 80,
                Status = TaskState.InProgress,
                TeamId = team.Id,
                EmployeeId = employee.Id
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Complex Task");
            result.Description.Should().Be("Detailed description");
            result.Difficulty.Should().Be(7.5);
            result.RequiredSkills.Should().BeEquivalentTo(new[] { "C#", "React", "SQL" });
            result.DeadlineDays.Should().Be(14);
            result.EstimatedHours.Should().Be(80);
            result.Status.Should().Be(TaskState.InProgress);

            result.Team.Should().NotBeNull();
            result.Team!.Name.Should().Be("Development Team");
            result.Employee.Should().NotBeNull();
            result.Employee!.Name.Should().Be("Developer");
        }

        [Fact]
        public async Task GetByIdAsync_WhenTaskNotExists_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByTeamIdAsync Tests

        [Fact]
        public async Task GetByTeamIdAsync_ShouldReturnTasksFromSpecificTeam()
        {
            // Arrange
            var team1 = new Team { Name = "Team 1" };
            var team2 = new Team { Name = "Team 2" };
            await _context.Teams.AddRangeAsync(team1, team2);
            await _context.SaveChangesAsync();

            var employee1 = new Employee { Name = "Employee 1", TeamId = team1.Id };
            var employee2 = new Employee { Name = "Employee 2", TeamId = team2.Id };
            await _context.Employees.AddRangeAsync(employee1, employee2);
            await _context.SaveChangesAsync();

            var task1 = new TaskItem { Title = "Task 1", TeamId = team1.Id, EmployeeId = employee1.Id, Status = TaskState.New };
            var task2 = new TaskItem { Title = "Task 2", TeamId = team1.Id, Status = TaskState.InProgress };
            var task3 = new TaskItem { Title = "Task 3", TeamId = team2.Id, EmployeeId = employee2.Id, Status = TaskState.Done };

            await _context.Tasks.AddRangeAsync(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByTeamIdAsync(team1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.TeamId == team1.Id);
            result.Should().Contain(t => t.Title == "Task 1");
            result.Should().Contain(t => t.Title == "Task 2");
            result.Should().NotContain(t => t.Title == "Task 3");

            // Verify navigation properties are loaded
            result.Should().OnlyContain(t => t.Team != null);
            result.Should().OnlyContain(t => t.Team!.Name == "Team 1");

            var assignedTask = result.First(t => t.Title == "Task 1");
            assignedTask.Employee.Should().NotBeNull();
            assignedTask.Employee!.Name.Should().Be("Employee 1");

            var unassignedTask = result.First(t => t.Title == "Task 2");
            unassignedTask.Employee.Should().BeNull();
        }

        [Fact]
        public async Task GetByTeamIdAsync_WhenNoTasksInTeam_ShouldReturnEmptyList()
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
        public async Task ExistsAsync_WhenTaskExists_ShouldReturnTrue()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Existing Task",
                TeamId = team.Id,
                Status = TaskState.New
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(task.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenTaskNotExists_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WithZeroId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(0);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WithNegativeId_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(-1);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTaskInDatabase()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee { Name = "Developer", TeamId = team.Id };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Original Title",
                Description = "Original Description",
                Difficulty = 3.0,
                RequiredSkills = new List<string> { "C#" },
                DeadlineDays = 5,
                EstimatedHours = 20,
                Status = TaskState.New,
                TeamId = team.Id
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Modify task
            task.Title = "Updated Title";
            task.Description = "Updated Description";
            task.Difficulty = 7.0;
            task.RequiredSkills = new List<string> { "C#", "React", "SQL" };
            task.DeadlineDays = 10;
            task.EstimatedHours = 50;
            task.Status = TaskState.InProgress;
            task.EmployeeId = employee.Id;

            // Act
            var result = await _repository.UpdateAsync(task.Id, task);

            // Assert
            result.Should().Be(1);

            // Verify in database
            var taskInDb = await _context.Tasks.FindAsync(task.Id);
            taskInDb.Should().NotBeNull();
            taskInDb!.Title.Should().Be("Updated Title");
            taskInDb.Description.Should().Be("Updated Description");
            taskInDb.Difficulty.Should().Be(7.0);
            taskInDb.RequiredSkills.Should().BeEquivalentTo(new[] { "C#", "React", "SQL" });
            taskInDb.DeadlineDays.Should().Be(10);
            taskInDb.EstimatedHours.Should().Be(50);
            taskInDb.Status.Should().Be(TaskState.InProgress);
            taskInDb.EmployeeId.Should().Be(employee.Id);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedTaskRows()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Test Task",
                TeamId = team.Id,
                Status = TaskState.New
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            task.Title = "Modified Task";

            // Act
            var result = await _repository.UpdateAsync(task.Id, task);

            // Assert
            result.Should().Be(1);
        }


        [Fact]
        public async Task UpdateAsync_UnassignEmployee_ShouldUpdateCorrectly()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee { Name = "Developer", TeamId = team.Id };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Assigned Task",
                TeamId = team.Id,
                EmployeeId = employee.Id,
                Status = TaskState.InProgress
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Unassign employee
            task.EmployeeId = null;
            task.Status = TaskState.New;

            // Act
            var result = await _repository.UpdateAsync(task.Id, task);

            // Assert
            var taskInDb = await _context.Tasks.FindAsync(task.Id);
            taskInDb!.EmployeeId.Should().BeNull();
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTaskExists_ShouldRemoveFromDatabase()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Task to Delete",
                TeamId = team.Id,
                Status = TaskState.New
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var taskId = task.Id;

            // Act
            await _repository.DeleteAsync(taskId);

            // Assert
            var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            taskInDb.Should().BeNull();

            var exists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskNotExists_ShouldNotThrowException()
        {
            // Act & Assert
            var act = async () => await _repository.DeleteAsync(999);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteAsync_WithAssignedEmployee_ShouldDeleteSuccessfully()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee { Name = "Developer", TeamId = team.Id };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Assigned Task",
                TeamId = team.Id,
                EmployeeId = employee.Id,
                Status = TaskState.InProgress
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var taskId = task.Id;

            // Act
            await _repository.DeleteAsync(taskId);

            // Assert
            var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
            taskInDb.Should().BeNull();

            // Employee should still exist
            var employeeInDb = await _context.Employees.FindAsync(employee.Id);
            employeeInDb.Should().NotBeNull();
        }

        #endregion
    }
}
