using FluentAssertions;
using TaskMind.Application.Services;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Tests
{
    public class WorkloadCalculationServiceTests
    {
        private readonly WorkloadCalculationService _workloadService;

        public WorkloadCalculationServiceTests()
        {
            _workloadService = new WorkloadCalculationService();
        }

        #region CalculateWorkloadChange Tests

        [Fact]
        public void CalculateWorkloadChange_ShouldCalculateCorrectly()
        {
            // Arrange
            var task = new TaskItem
            {
                EstimatedHours = 16, // sqrt(16) = 4
                Difficulty = 5,
                DeadlineDays = 10
            };
            var employee = new Employee
            {
                TaskCompletionSpeed = 8.0
            };

            // Expected: (4 * 5) / ((10 * 8) + 1) = 20 / 81 ≈ 0.247

            // Act
            var result = _workloadService.CalculateWorkloadChange(task, employee);

            // Assert
            result.Should().BeApproximately(0.247, 0.01);
        }

        [Fact]
        public void CalculateWorkloadChange_WithZeroEstimatedHours_ShouldReturnZero()
        {
            // Arrange
            var task = new TaskItem
            {
                EstimatedHours = 0,
                Difficulty = 5,
                DeadlineDays = 10
            };
            var employee = new Employee
            {
                TaskCompletionSpeed = 8.0
            };

            // Act
            var result = _workloadService.CalculateWorkloadChange(task, employee);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void CalculateWorkloadChange_WithHighDifficulty_ShouldReturnHigherWorkload()
        {
            // Arrange
            var easyTask = new TaskItem
            {
                EstimatedHours = 9, // sqrt(9) = 3
                Difficulty = 2,
                DeadlineDays = 5
            };
            var hardTask = new TaskItem
            {
                EstimatedHours = 9, // sqrt(9) = 3
                Difficulty = 8,
                DeadlineDays = 5
            };
            var employee = new Employee
            {
                TaskCompletionSpeed = 6.0
            };

            // Act
            var easyWorkload = _workloadService.CalculateWorkloadChange(easyTask, employee);
            var hardWorkload = _workloadService.CalculateWorkloadChange(hardTask, employee);

            // Assert
            hardWorkload.Should().BeGreaterThan(easyWorkload);
        }

        [Fact]
        public void CalculateWorkloadChange_WithFasterEmployee_ShouldReturnLowerWorkload()
        {
            // Arrange
            var task = new TaskItem
            {
                EstimatedHours = 25, // sqrt(25) = 5
                Difficulty = 4,
                DeadlineDays = 7
            };
            var slowEmployee = new Employee
            {
                TaskCompletionSpeed = 3.0
            };
            var fastEmployee = new Employee
            {
                TaskCompletionSpeed = 9.0
            };

            // Act
            var slowWorkload = _workloadService.CalculateWorkloadChange(task, slowEmployee);
            var fastWorkload = _workloadService.CalculateWorkloadChange(task, fastEmployee);

            // Assert
            fastWorkload.Should().BeLessThan(slowWorkload);
        }

        #endregion
    }
}
