using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiTrainApp.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingPlanSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduleMask",
                table: "TrainingPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleMask",
                table: "TrainingPlans");
        }
    }
}
