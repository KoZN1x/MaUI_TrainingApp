using Microsoft.Maui.Controls;

namespace MauiTrainApp.Controls
{
    public partial class StatTile : ContentView
    {
        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(string), typeof(StatTile), string.Empty);

        public static readonly BindableProperty CaptionProperty =
            BindableProperty.Create(nameof(Caption), typeof(string), typeof(StatTile), string.Empty);

        public static readonly BindableProperty DeltaProperty =
            BindableProperty.Create(nameof(Delta), typeof(string), typeof(StatTile), string.Empty, propertyChanged: OnDeltaChanged);

        public static readonly BindableProperty HasDeltaProperty =
            BindableProperty.Create(nameof(HasDelta), typeof(bool), typeof(StatTile), false);

        public StatTile()
        {
            InitializeComponent();
        }

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public string Delta
        {
            get => (string)GetValue(DeltaProperty);
            set => SetValue(DeltaProperty, value);
        }

        public bool HasDelta
        {
            get => (bool)GetValue(HasDeltaProperty);
            private set => SetValue(HasDeltaProperty, value);
        }

        private static void OnDeltaChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is StatTile tile)
            {
                tile.HasDelta = !string.IsNullOrWhiteSpace(newValue as string);
            }
        }
    }
}
