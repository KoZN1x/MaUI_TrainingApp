using MauiTrainApp.ViewModels;
using MauiTrainApp.Views.Base;

namespace MauiTrainApp.Views
{
    public partial class PlanEditorPage : ContentPageBase
    {
        public PlanEditorPage(PlanEditorViewModel viewModel)
            : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
