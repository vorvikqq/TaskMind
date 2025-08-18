using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMind.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixModelsAndAddJsonConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Workload",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "Complexity",
                table: "Tasks",
                newName: "EstimatedHours");

            migrationBuilder.AddColumn<int>(
                name: "DeadlineDays",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Difficulty",
                table: "Tasks",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "CurrentWorkload",
                table: "Employees",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "TaskCompletionSpeed",
                table: "Employees",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadlineDays",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CurrentWorkload",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TaskCompletionSpeed",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "EstimatedHours",
                table: "Tasks",
                newName: "Complexity");

            migrationBuilder.AddColumn<int>(
                name: "Workload",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
