using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class HistoryPage : ContentPageBase
    {
        public HistoryPage(HistoryViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
