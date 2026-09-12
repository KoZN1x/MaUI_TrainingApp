namespace MauiTrainApp.Navigation.Interfaces
{
    public interface INavigator
    {
        Task GoToWorkoutAsync(Guid workoutId);

        Task GoBackAsync();
    }
}
