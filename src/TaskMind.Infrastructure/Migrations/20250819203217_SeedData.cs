using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Teams",
            columns: new[] { "Id", "Name" },
            values: new object[,]
            {
                { 2, "League" },
                { 4, "Team1" },
                { 6, "Telepuzik" }
            });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Name", "Skills", "TeamId", "CurrentWorkload", "TaskCompletionSpeed" },
                values: new object[,]
                {
                    { 14, "Victor", "[\"C#\",\"ASP.NET\"]", 6, 0.5, 0.8 },
                    { 15, "Petro", "[\"Python\",\"Django\",\"SQL\"]", 6, 0.3, 0.8 },
                    { 16, "Kate", "[\"Python\",\"Django\"]", 6, 0.2, 0.7 },
                    { 17, "Danya", "[\"Java\",\"SQL\"]", 6, 0.4, 0.9 },
                    { 18, "Den", "[\"JavaScript\",\"React\",\"Node.js\"]", 6, 0.3, 0.75 },
                    { 19, "Andrew", "[\"Python\",\"SQL\"]", 6, 0.6, 0.8 },
                    { 20, "Artem", "[\"C#\",\"SQL\"]", 6, 0.2, 0.6 },
                    { 21, "Vlad", "[\"Node.js\",\"Angular\"]", 6, 0.5, 0.7 },
                    { 22, "Ivan", "[\"JavaScript\",\"React\"]", 6, 0.3, 0.8 },
                    { 23, "Roman", "[\"Python\",\"Django\",\"Java\"]", 6, 0.4, 0.8 },
                    { 26, "Kevin", "[\"C#\",\"ASP.NET\"]", 6, 0.3, 0.7 }
                });

            migrationBuilder.InsertData(
                 table: "Tasks",
                 columns: new[] { "Id", "Title", "Description", "Status", "TeamId", "EmployeeId", "EstimatedHours", "DeadlineDays", "Difficulty", "RequiredSkills" },
                 values: new object[,]
                 {
                    { 8, "Develop API with Python", "Create a RESTful API using Python and Django, with proper endpoints for CRUD operations.", 1, 6, null, 20, 10, 0.6, "[\"Python\",\"Django\"]" },
                    { 9, "Build Web Application with C#", "Develop a web application using ASP.NET and C#, implementing user authentication and role management.", 1, 6, null, 15, 7, 0.5, "[\"C#\",\"ASP.NET\"]" },
                    { 10, "Database Optimization with Java", "Optimize SQL queries and database schema for an existing Java application to improve performance.", 1, 6, null, 25, 13, 0.8, "[\"Java\",\"SQL\"]" },
                    { 11, "Frontend Development with React", "Create a dynamic, responsive frontend for a web application using React and JavaScript.", 1, 6, null, 18, 9, 0.6, "[\"JavaScript\",\"React\"]" },
                    { 12, "Develop a Data Processing Script with Python and SQL", "Build a Python script that processes and analyzes data using SQL queries. The task involves optimizing database interactions and ensuring efficient execution of queries within the given deadline.", 1, 6, null, 35, 15, 0.9, "[\"Python\",\"SQL\"]" }
                 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Id",
                keyValues: new object[] { 2, 4, 6 });

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValues: new object[] { 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 26 });

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValues: new object[] { 8, 9, 10, 11, 12 });

        }
    }
}
