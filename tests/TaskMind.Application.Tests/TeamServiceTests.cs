using FluentAssertions;
using Moq;
using TaskMind.Application.DTOs.Team;
using TaskMind.Application.Repositories.Interfaces;
using TaskMind.Application.Services;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Tests
{
    public class TeamServiceTests
    {
        private readonly Mock<ITeamRepository> _mockTeamRepo;
        private readonly Mock<ITaskItemRepository> _mockTaskItemRepo;
        private readonly TeamService _teamService;

        public TeamServiceTests()
        {
            _mockTeamRepo = new Mock<ITeamRepository>();
            _mockTaskItemRepo = new Mock<ITaskItemRepository>();
            _teamService = new TeamService(_mockTeamRepo.Object, _mockTaskItemRepo.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenTeamsExist_ShouldReturnAllTeams()
        {
            // Arrange
            var expectedTeams = new List<Team>
        {
            new() { Id = 1, Name = "Frontend Team" },
            new() { Id = 2, Name = "Backend Team" },
            new() { Id = 3, Name = "DevOps Team" }
        };
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(expectedTeams);

            // Act
            var result = await _teamService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(expectedTeams);
            _mockTeamRepo.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTeams_ShouldReturnEmptyCollection()
        {
            // Arrange
            _mockTeamRepo.Setup(x => x.GetAllAsync())
                        .ReturnsAsync(new List<Team>());

            // Act
            var result = await _teamService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenTeamExists_ShouldReturnTeam()
        {
            // Arrange
            var teamId = 1;
            var expectedTeam = new Team { Id = teamId, Name = "Frontend Team" };
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync(expectedTeam);

            // Act
            var result = await _teamService.GetByIdAsync(teamId);

            // Assert
            result.Should().BeEquivalentTo(expectedTeam);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTeamNotExists_ShouldReturnNull()
        {
            // Arrange
            var teamId = 999;
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.GetByIdAsync(teamId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetForEditAsync Tests

        [Fact]
        public async Task GetForEditAsync_WhenTeamExists_ShouldReturnUpdateTeamDto()
        {
            // Arrange
            var teamId = 1;
            var team = new Team { Id = teamId, Name = "Frontend Team" };
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync(team);

            // Act
            var result = await _teamService.GetForEditAsync(teamId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(teamId);
            result.Name.Should().Be("Frontend Team");
        }

        [Fact]
        public async Task GetForEditAsync_WhenTeamNotExists_ShouldReturnNull()
        {
            // Arrange
            var teamId = 999;
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.GetForEditAsync(teamId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetTeamTasksAsync Tests

        [Fact]
        public async Task GetTeamTasksAsync_WhenTeamExists_ShouldReturnTeamTasksModel()
        {
            // Arrange
            var teamId = 1;
            var team = new Team { Id = teamId, Name = "Frontend Team" };
            var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task 1", TeamId = teamId },
            new() { Id = 2, Title = "Task 2", TeamId = teamId }
        };

            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync(team);
            _mockTaskItemRepo.Setup(x => x.GetByTeamIdAsync(teamId))
                            .ReturnsAsync(tasks);

            // Act
            var result = await _teamService.GetTeamTasksAsync(teamId);

            // Assert
            result.Should().NotBeNull();
            result!.Team.Should().BeEquivalentTo(team);
            result.Tasks.Should().BeEquivalentTo(tasks);
            result.Tasks.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTeamTasksAsync_WhenTeamNotExists_ShouldReturnNull()
        {
            // Arrange
            var teamId = 999;
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync((Team?)null);

            // Act
            var result = await _teamService.GetTeamTasksAsync(teamId);

            // Assert
            result.Should().BeNull();
            _mockTaskItemRepo.Verify(x => x.GetByTeamIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetTeamTasksAsync_WhenTeamExistsButNoTasks_ShouldReturnModelWithEmptyTasks()
        {
            // Arrange
            var teamId = 1;
            var team = new Team { Id = teamId, Name = "Frontend Team" };
            var emptyTasks = new List<TaskItem>();

            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync(team);
            _mockTaskItemRepo.Setup(x => x.GetByTeamIdAsync(teamId))
                            .ReturnsAsync(emptyTasks);

            // Act
            var result = await _teamService.GetTeamTasksAsync(teamId);

            // Assert
            result.Should().NotBeNull();
            result!.Team.Should().BeEquivalentTo(team);
            result.Tasks.Should().BeEmpty();
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ShouldCallRepositoryCreate()
        {
            // Arrange
            var createDto = new CreateTeamRequest { Name = "New Team" };

            // Act
            await _teamService.CreateAsync(createDto);

            // Assert
            _mockTeamRepo.Verify(x => x.CreateAsync(It.Is<Team>(t => t.Name == "New Team")), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateTeamWithCorrectName()
        {
            // Arrange
            var createDto = new CreateTeamRequest { Name = "Quality Assurance Team" };
            Team? capturedTeam = null;
            _mockTeamRepo.Setup(x => x.CreateAsync(It.IsAny<Team>()))
                        .Callback<Team>(team => capturedTeam = team);

            // Act
            await _teamService.CreateAsync(createDto);

            // Assert
            capturedTeam.Should().NotBeNull();
            capturedTeam!.Name.Should().Be("Quality Assurance Team");
            capturedTeam.Id.Should().Be(0); // default value for new entity
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenTeamExists_ShouldUpdateTeam()
        {
            // Arrange
            var updateDto = new UpdateTeamRequest { Id = 1, Name = "Updated Team Name" };
            var existingTeam = new Team { Id = 1, Name = "Old Team Name" };

            _mockTeamRepo.Setup(x => x.GetByIdAsync(updateDto.Id))
                        .ReturnsAsync(existingTeam);

            // Act
            await _teamService.UpdateAsync(updateDto);

            // Assert
            existingTeam.Name.Should().Be("Updated Team Name");
            _mockTeamRepo.Verify(x => x.GetByIdAsync(updateDto.Id), Times.Once);
            _mockTeamRepo.Verify(x => x.UpdateAsync(existingTeam.Id, existingTeam), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenTeamNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateTeamRequest { Id = 999, Name = "Non-existent Team" };
            _mockTeamRepo.Setup(x => x.GetByIdAsync(updateDto.Id))
                        .ReturnsAsync((Team?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamService.UpdateAsync(updateDto));

            exception.Message.Should().Be("Team not found");
            _mockTeamRepo.Verify(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<Team>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenTeamExists_ShouldDeleteTeam()
        {
            // Arrange
            var teamId = 1;
            var existingTeam = new Team { Id = teamId, Name = "Team to Delete" };
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync(existingTeam);

            // Act
            await _teamService.DeleteAsync(teamId);

            // Assert
            _mockTeamRepo.Verify(x => x.GetByIdAsync(teamId), Times.Once);
            _mockTeamRepo.Verify(x => x.DeleteAsync(teamId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenTeamNotExists_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var teamId = 999;
            _mockTeamRepo.Setup(x => x.GetByIdAsync(teamId))
                        .ReturnsAsync((Team?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _teamService.DeleteAsync(teamId));

            exception.Message.Should().Be("Team not found");
            _mockTeamRepo.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenTeamExists_ShouldReturnTrue()
        {
            // Arrange
            var teamId = 1;
            _mockTeamRepo.Setup(x => x.ExistsAsync(teamId))
                        .ReturnsAsync(true);

            // Act
            var result = await _teamService.ExistsAsync(teamId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenTeamNotExists_ShouldReturnFalse()
        {
            // Arrange
            var teamId = 999;
            _mockTeamRepo.Setup(x => x.ExistsAsync(teamId))
                        .ReturnsAsync(false);

            // Act
            var result = await _teamService.ExistsAsync(teamId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
