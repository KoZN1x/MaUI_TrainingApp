using Microsoft.Maui.Graphics;

namespace MauiTrainApp.Controls
{
    public sealed record BarChartBar(
        string Label,
        string ValueText,
        double BarHeight,
        double ChartHeight,
        double BarWidth,
        Color Fill);
}
