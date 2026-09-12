using System.Collections;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiTrainApp.Controls
{
    public partial class BarChartView : ContentView
    {
        private const double MinimumBarHeight = 3;

        private static readonly Color BarColor = Color.FromArgb("#5D5294");
        private static readonly Color HighlightColor = Color.FromArgb("#9184D9");

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(BarChartView), propertyChanged: OnLayoutChanged);

        public static readonly BindableProperty ChartHeightProperty =
            BindableProperty.Create(nameof(ChartHeight), typeof(double), typeof(BarChartView), 110d, propertyChanged: OnLayoutChanged);

        public static readonly BindableProperty BarWidthProperty =
            BindableProperty.Create(nameof(BarWidth), typeof(double), typeof(BarChartView), 28d, propertyChanged: OnLayoutChanged);

        public BarChartView()
        {
            InitializeComponent();
        }

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public double ChartHeight
        {
            get => (double)GetValue(ChartHeightProperty);
            set => SetValue(ChartHeightProperty, value);
        }

        public double BarWidth
        {
            get => (double)GetValue(BarWidthProperty);
            set => SetValue(BarWidthProperty, value);
        }

        private static void OnLayoutChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as BarChartView)?.Rebuild();
        }

        private void Rebuild()
        {
            var items = ItemsSource?.OfType<BarChartItem>().ToList() ?? [];

            if (items.Count == 0)
            {
                BindableLayout.SetItemsSource(Bars, Array.Empty<BarChartBar>());

                return;
            }

            var maximum = items.Max(x => x.Value);

            BindableLayout.SetItemsSource(Bars, items.Select(x => new BarChartBar(
                x.Label,
                x.ValueText ?? string.Empty,
                ScaleBar(x.Value, maximum),
                ChartHeight,
                BarWidth,
                x.IsHighlighted ? HighlightColor : BarColor)).ToList());
        }

        private double ScaleBar(double value, double maximum)
        {
            if (maximum <= 0 || value <= 0)
            {
                return MinimumBarHeight;
            }

            return Math.Max(MinimumBarHeight, value / maximum * ChartHeight);
        }
    }
}
