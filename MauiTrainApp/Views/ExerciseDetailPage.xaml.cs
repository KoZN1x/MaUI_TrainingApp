using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class ExerciseDetailPage : ContentPageBase
    {
        public ExerciseDetailPage(ExerciseDetailViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
