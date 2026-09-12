using MauiTrainApp.ViewModels.Base;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Views.Base
{
    public abstract class ContentPageBase : ContentPage
    {
        private readonly ViewModelBase _viewModel;

        protected ContentPageBase(ViewModelBase viewModel)
        {
            _viewModel = viewModel;

            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.AppearingAsync();
        }
    }
}
