using MauiTrainApp.ViewModels;
using MauiTrainApp.ViewModels.Items;
using MauiTrainApp.Views.Base;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Views
{
    public partial class WorkoutPage : ContentPageBase
    {
        public WorkoutPage(WorkoutViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
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
    }
}
