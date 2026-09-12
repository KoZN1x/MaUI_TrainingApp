using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class WorkoutDetailPage : ContentPageBase
    {
        public WorkoutDetailPage(WorkoutDetailViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
