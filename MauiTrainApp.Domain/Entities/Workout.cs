using MauiTrainApp.Domain.Interfaces;

namespace MauiTrainApp.Domain.Entities
{
    public sealed record Workout : IEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public DateTime CreatedAt {  get; init; } = DateTime.Now;

        public DateTime? UpdatedAt { get; private set; }

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public DateOnly WorkoutDay { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

        public void SetDescription(string description)
        {
            Description = description;
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetUpdatedTime()
        {
            UpdatedAt = DateTime.Now;
        }

        public void SetWorkoutDay(DateOnly workoutDay)
        {
            WorkoutDay = workoutDay;
        }

    }
}
