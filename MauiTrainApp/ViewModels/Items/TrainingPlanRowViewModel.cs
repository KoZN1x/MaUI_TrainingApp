using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Formatting;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record TrainingPlanRowViewModel(
        Guid Id,
        string Name,
        string SummaryText,
        string ScheduleText,
        IReadOnlyList<string> ExerciseNames,
        bool IsNext)
    {
        private const int MinutesPerWorkingSet = 3;
        private const int MinutesRounding = 5;

        public bool HasSchedule => ScheduleText.Length > 0;

        public static TrainingPlanRowViewModel From(TrainingPlanDetailsReadModel trainingPlan, bool isNext)
        {
            var workingSetCount = trainingPlan.ExerciseSets.Sum(x => x.WorkingSets.Count);

            var summary = string.Join(" · ",
                RussianPlural.Exercises(trainingPlan.ExerciseSets.Count),
                RussianPlural.WorkingSets(workingSetCount),
                $"~{EstimateMinutes(workingSetCount)} мин");

            return new TrainingPlanRowViewModel(
                trainingPlan.Id,
                trainingPlan.Name,
                summary,
                trainingPlan.Schedule.IsEmpty ? string.Empty : WeekDays.Describe(trainingPlan.Schedule),
                [.. trainingPlan.ExerciseSets.Select(x => x.ExerciseName)],
                isNext);
        }

        private static int EstimateMinutes(int workingSetCount)
        {
            var minutes = workingSetCount * MinutesPerWorkingSet;

            return Math.Max(MinutesRounding, (int)Math.Round(minutes / (double)MinutesRounding) * MinutesRounding);
        }
    }
}
