using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiTrainApp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMuscleGroupAndWorkoutDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Workouts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MuscleGroup",
                table: "Exercises",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "MuscleGroup",
                table: "Exercises");
        }
    }
}
