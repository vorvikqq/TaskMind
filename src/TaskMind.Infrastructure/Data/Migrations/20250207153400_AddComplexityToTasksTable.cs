using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMind.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplexityToTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Complexity",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complexity",
                table: "Tasks");
        }
    }
}
