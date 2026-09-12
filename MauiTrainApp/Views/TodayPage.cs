using MauiTrainApp.Navigation.Interfaces;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public sealed class TodayPage : StubPageBase
    {
        public TodayPage(INavigator navigator)
            : base("Сегодня")
        {
            AddLink("Тренировка", () => navigator.GoToWorkoutAsync(Guid.Empty));
            AddLink("Прошлая тренировка", () => navigator.GoToWorkoutDetailsAsync(Guid.Empty));
            AddLink("Прогресс", navigator.GoToProgressAsync);
            AddLink("Упражнения", navigator.GoToExercisesAsync);
            AddLink("Карточка упражнения", () => navigator.GoToExerciseDetailsAsync(Guid.Empty));
            AddLink("Редактор плана", () => navigator.GoToPlanEditorAsync());
        }
    }
}
