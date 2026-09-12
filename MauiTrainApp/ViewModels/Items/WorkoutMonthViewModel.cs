namespace MauiTrainApp.ViewModels.Items
{
    public sealed record WorkoutMonthViewModel(
        string Title,
        string SummaryText,
        IReadOnlyList<WorkoutRowViewModel> Workouts);
}
