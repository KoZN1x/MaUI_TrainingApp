using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiTrainApp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseSetPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "ExerciseSets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE ExerciseSets
                SET Position = (
                    SELECT COUNT(*)
                    FROM ExerciseSets AS peers
                    WHERE ((ExerciseSets.TrainingPlanRecordId IS NOT NULL
                            AND peers.TrainingPlanRecordId = ExerciseSets.TrainingPlanRecordId)
                        OR (ExerciseSets.WorkoutRecordId IS NOT NULL
                            AND peers.WorkoutRecordId = ExerciseSets.WorkoutRecordId))
                      AND (peers.CreatedAt < ExerciseSets.CreatedAt
                        OR (peers.CreatedAt = ExerciseSets.CreatedAt AND peers.Id < ExerciseSets.Id))
                )
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "ExerciseSets");
        }
    }
}
