using MauiTrainApp.Converters;
using MauiTrainApp.Domain.ReadModels;
using MauiTrainApp.Formatting;

namespace MauiTrainApp.ViewModels.Items
{
    public sealed record MuscleShareViewModel(
        string Name,
        string VolumeText,
        string ShareText,
        double Share)
    {
        public static MuscleShareViewModel From(MuscleVolumeReadModel muscle, double totalVolume)
        {
            var share = totalVolume <= 0 ? 0 : muscle.Volume / totalVolume;

            return new MuscleShareViewModel(
                MuscleGroupConverter.ToName(muscle.MuscleGroup),
                $"{VolumeConverter.ToText(muscle.Volume)} · {RussianPlural.WorkingSets(muscle.WorkingSetCount)}",
                $"{Math.Round(share * 100)} %",
                share);
        }
    }
}
