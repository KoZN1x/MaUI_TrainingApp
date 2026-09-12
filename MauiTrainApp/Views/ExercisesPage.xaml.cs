using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class ExercisesPage : ContentPageBase
    {
        public ExercisesPage(ExercisesViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
