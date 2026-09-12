using MauiTrainApp.Startup;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace MauiTrainApp
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly AppStartup _startup;

        public App(AppStartup startup)
        {
            _startup = startup;

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnStart()
        {
            base.OnStart();

            _ = _startup.EnsureReadyAsync();
        }
    }
}
