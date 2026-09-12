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

        private void OnWorkingSetUnfocused(object? sender, FocusEventArgs e)
        {
            if (sender is not BindableObject { BindingContext: WorkingSetViewModel workingSet })
            {
                return;
            }

            if (BindingContext is WorkoutViewModel viewModel
                && viewModel.SaveWorkingSetCommand.CanExecute(workingSet))
            {
                viewModel.SaveWorkingSetCommand.Execute(workingSet);
            }
        }
    }
}
