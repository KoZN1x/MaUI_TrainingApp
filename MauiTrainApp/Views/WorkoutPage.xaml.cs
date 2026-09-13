using MauiTrainApp.ViewModels;
using MauiTrainApp.ViewModels.Items;
using MauiTrainApp.Views.Base;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Views
{
    public partial class WorkoutPage : ContentPageBase
    {
        private const int ExpandSettleMilliseconds = 120;

        public WorkoutPage(WorkoutViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();

            viewModel.ExerciseExpanded += OnExerciseExpanded;
        }

        private void OnWorkingSetEntryCompleted(object? sender, EventArgs e)
        {
            Commit(sender);
        }

        private void OnWorkingSetEntryUnfocused(object? sender, FocusEventArgs e)
        {
            Commit(sender);
        }

        private void Commit(object? sender)
        {
            if (sender is Entry { BindingContext: WorkingSetViewModel workingSet }
                && BindingContext is WorkoutViewModel viewModel)
            {
                viewModel.EditWorkingSetCommand.Execute(workingSet);
            }
        }

        private async void OnExerciseExpanded(object? sender, WorkoutExerciseViewModel exercise)
        {
            var card = ExercisesLayout.Children
                .OfType<VisualElement>()
                .FirstOrDefault(x => ReferenceEquals(x.BindingContext, exercise));

            if (card is null)
            {
                return;
            }

            await Task.Delay(ExpandSettleMilliseconds);

            try
            {
                await ExercisesScroll.ScrollToAsync(card, ScrollToPosition.Start, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
