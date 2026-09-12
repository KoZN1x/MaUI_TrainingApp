using MauiTrainApp.Controls;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Views
{
    public partial class ProgressPage : ContentPage
    {
        public ProgressPage()
        {
            InitializeComponent();

            WeeklyVolume.ItemsSource = new List<BarChartItem>
            {
                new("29 июл", 8200, "8,2 т"),
                new("5 авг", 9400, "9,4 т"),
                new("12 авг", 7100, "7,1 т"),
                new("19 авг", 10600, "10,6 т"),
                new("26 авг", 11200, "11,2 т"),
                new("2 сен", 9900, "9,9 т"),
                new("9 сен", 12400, "12,4 т", true)
            };
        }
    }
}
