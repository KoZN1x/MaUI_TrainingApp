using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class TodayPage : ContentPageBase
    {
        public TodayPage(TodayViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
