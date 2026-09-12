using MauiTrainApp.ViewModels;
using MauiTrainApp.ViewModels.Items;
using MauiTrainApp.Views.Base;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Views
{
    public partial class PlanEditorPage : ContentPageBase
    {
        public PlanEditorPage(PlanEditorViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }

        private void OnPlannedEntryCompleted(object? sender, EventArgs e)
        {
            Commit(sender);
        }

        private void OnPlannedEntryUnfocused(object? sender, FocusEventArgs e)
        {
            Commit(sender);
        }

        private void Commit(object? sender)
        {
            if (sender is Entry { BindingContext: PlannedExerciseViewModel exercise }
                && BindingContext is PlanEditorViewModel viewModel)
            {
                viewModel.EditPlannedExerciseCommand.Execute(exercise);
            }
        }
    }
}
