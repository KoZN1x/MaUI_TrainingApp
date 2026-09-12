namespace MauiTrainApp.Navigation.Interfaces
{
    public interface INavigator
    {
        Task GoToTodayAsync();

        Task GoToTrainingPlansAsync();

        Task GoToHistoryAsync();

        Task GoToWorkoutAsync(Guid workoutId);

        Task GoToWorkoutDetailsAsync(Guid workoutId);

        Task GoToPlanEditorAsync(Guid? trainingPlanId = null);

        Task GoToExercisesAsync();

        Task GoToExerciseDetailsAsync(Guid exerciseId);

        Task GoToProgressAsync();

        Task GoBackAsync();
    }
}
