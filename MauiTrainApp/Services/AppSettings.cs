using MauiTrainApp.Services.Interfaces;
using Microsoft.Maui.Storage;

namespace MauiTrainApp.Services
{
    internal sealed class AppSettings : IAppSettings
    {
        private const string WeightStepKey = "weight_step";
        private const double DefaultWeightStep = 2.5;

        public double WeightStep
        {
            get
            {
                var stored = Preferences.Default.Get(WeightStepKey, DefaultWeightStep);

                return stored > 0 ? stored : DefaultWeightStep;
            }
            set => Preferences.Default.Set(WeightStepKey, value);
        }
    }
}
