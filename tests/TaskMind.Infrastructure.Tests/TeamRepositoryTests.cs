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
    public class TeamRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TeamRepository _repository;
        private readonly DbConnection _connection;

        public TeamRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Database.EnsureCreated();

            _context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
            _repository = new TeamRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldAddTeamToDatabase()
        {
            // Arrange
            var team = new Team
            {
                Name = "Development Team"
            };

            // Act
            var result = await _repository.CreateAsync(team);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("Development Team");

            var teamInDb = await _context.Teams.FindAsync(result.Id);
            teamInDb.Should().NotBeNull();
            teamInDb!.Name.Should().Be("Development Team");
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnTeamWithGeneratedId()
        {
            // Arrange
            var team = new Team
            {
                Name = "QA Team"
            };

            // Act
            var result = await _repository.CreateAsync(team);

            // Assert
            result.Id.Should().BeGreaterThan(0);
            team.Id.Should().Be(result.Id); // Original object should be updated
        }


        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenNoTeams_ShouldReturnEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTeamsWithEmployeesAndTasks()
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

            var task1 = new TaskItem
            {
                Title = "Frontend Task",
                TeamId = team1.Id,
                Status = TaskState.InProgress
            };
            var task2 = new TaskItem
            {
                Title = "Backend Task",
                TeamId = team2.Id,
                Status = TaskState.New
            };

            await _context.Employees.AddRangeAsync(employee1, employee2);
            await _context.Tasks.AddRangeAsync(task1, task2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);

            var frontendTeam = result.First(t => t.Name == "Frontend Team");
            frontendTeam.Employees.Should().HaveCount(1);
            frontendTeam.Employees.First().Name.Should().Be("John Doe");
            frontendTeam.Tasks.Should().HaveCount(1);
            frontendTeam.Tasks.First().Title.Should().Be("Frontend Task");

            var backendTeam = result.First(t => t.Name == "Backend Team");
            backendTeam.Employees.Should().HaveCount(1);
            backendTeam.Employees.First().Name.Should().Be("Jane Smith");
            backendTeam.Tasks.Should().HaveCount(1);
            backendTeam.Tasks.First().Title.Should().Be("Backend Task");
        }

        [Fact]
        public async Task GetAllAsync_WithTeamsWithoutEmployeesOrTasks_ShouldReturnEmptyCollections()
        {
            // Arrange
            var team = new Team { Name = "Empty Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(1);
            var emptyTeam = result.First();
            emptyTeam.Name.Should().Be("Empty Team");
            emptyTeam.Employees.Should().NotBeNull();
            emptyTeam.Employees.Should().BeEmpty();
            emptyTeam.Tasks.Should().NotBeNull();
            emptyTeam.Tasks.Should().BeEmpty();
        }


        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTeamExists_ShouldReturnTeamWithEmployeesAndTasks()
        {
            // Arrange
            var team = new Team { Name = "Test Team" };
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
            var task = new TaskItem
            {
                Title = "Test Task",
                Description = "Test task description",
                TeamId = team.Id,
                Status = TaskState.InProgress
            };

            await _context.Employees.AddAsync(employee);
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Team");

            result.Employees.Should().HaveCount(1);
            result.Employees.First().Name.Should().Be("Test Employee");
            result.Employees.First().Skills.Should().BeEquivalentTo(new[] { "C#", "SQL" });

            result.Tasks.Should().HaveCount(1);
            result.Tasks.First().Title.Should().Be("Test Task");
            result.Tasks.First().Description.Should().Be("Test task description");
            result.Tasks.First().Status.Should().Be(TaskState.InProgress);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTeamNotExists_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithTeamWithoutEmployeesOrTasks_ShouldReturnTeamWithEmptyCollections()
        {
            // Arrange
            var team = new Team { Name = "Lonely Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(team.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Lonely Team");
            result.Employees.Should().NotBeNull();
            result.Employees.Should().BeEmpty();
            result.Tasks.Should().NotBeNull();
            result.Tasks.Should().BeEmpty();
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenTeamExists_ShouldReturnTrue()
        {
            // Arrange
            var team = new Team { Name = "Existing Team" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(team.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenTeamNotExists_ShouldReturnFalse()
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
        public async Task UpdateAsync_ShouldUpdateTeamInDatabase()
        {
            // Arrange
            var team = new Team
            {
                Name = "Original Name"
            };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            // Modify team
            team.Name = "Updated Name";

            // Act
            var result = await _repository.UpdateAsync(team.Id, team);

            // Assert
            result.Should().Be(1);

            // Verify in database
            var teamInDb = await _context.Teams.FindAsync(team.Id);
            teamInDb.Should().NotBeNull();
            teamInDb!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnUpdatedTeamRows()
        {
            // Arrange
            var team = new Team
            {
                Name = "Test Team"
            };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            team.Name = "Modified Team";

            // Act
            var result = await _repository.UpdateAsync(team.Id, team);

            // Assert
            result.Should().Be(1);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTeamExists_ShouldRemoveFromDatabase()
        {
            // Arrange
            var team = new Team
            {
                Name = "Team to Delete"
            };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var teamId = team.Id;

            // Act
            await _repository.DeleteAsync(teamId);

            // Assert
            var teamInDb = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            teamInDb.Should().BeNull();

            var exists = await _context.Teams.AnyAsync(t => t.Id == teamId);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenTeamNotExists_ShouldNotThrowException()
        {
            // Act & Assert
            var act = async () => await _repository.DeleteAsync(999);
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteAsync_WhenTeamNotExists_ShouldNotAffectOtherTeams()
        {
            // Arrange
            var team = new Team
            {
                Name = "Existing Team"
            };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var originalCount = await _context.Teams.CountAsync();

            // Act
            await _repository.DeleteAsync(999);

            // Assert
            var newCount = await _context.Teams.CountAsync();
            newCount.Should().Be(originalCount);

            var existingTeam = await _context.Teams.FindAsync(team.Id);
            existingTeam.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithAssociatedEmployees_ShouldHandleCorrectly()
        {
            // Arrange
            var team = new Team { Name = "Team with Employees" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var employee = new Employee
            {
                Name = "Team Member",
                Skills = new List<string> { "C#" },
                CurrentWorkload = 0.5,
                TaskCompletionSpeed = 8.0,
                TeamId = team.Id
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            var teamId = team.Id;

            // Act
            await _repository.DeleteAsync(teamId);

            // Assert
            var teamInDb = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            teamInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithAssociatedTasks_ShouldHandleCorrectly()
        {
            // Arrange
            var team = new Team { Name = "Team with Tasks" };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            var task = new TaskItem
            {
                Title = "Team Task",
                TeamId = team.Id,
                Status = TaskState.New
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var teamId = team.Id;

            // Act
            await _repository.DeleteAsync(teamId);

            // Assert
            var teamInDb = await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            teamInDb.Should().BeNull();

        }

        #endregion
    }
}
