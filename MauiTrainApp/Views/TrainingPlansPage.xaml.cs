using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class TrainingPlansPage : ContentPageBase
    {
        public TrainingPlansPage(TrainingPlansViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
