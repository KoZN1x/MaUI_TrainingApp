using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiTrainApp.Controls
{
    public partial class TagChip : ContentView
    {
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(TagChip), string.Empty);

        public static readonly BindableProperty KindProperty =
            BindableProperty.Create(nameof(Kind), typeof(TagChipKind), typeof(TagChip), TagChipKind.Neutral, propertyChanged: OnKindChanged);

        public TagChip()
        {
            InitializeComponent();

            ApplyKind();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public TagChipKind Kind
        {
            get => (TagChipKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        private static void OnKindChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as TagChip)?.ApplyKind();
        }

        private void ApplyKind()
        {
            switch (Kind)
            {
                case TagChipKind.Accent:
                    ChipBorder.BackgroundColor = Color.FromArgb("#2B2741");
                    ChipBorder.Stroke = Color.FromArgb("#5D5294");
                    ChipLabel.TextColor = Color.FromArgb("#D2CEFD");
                    break;

                case TagChipKind.Outline:
                    ChipBorder.BackgroundColor = Colors.Transparent;
                    ChipBorder.Stroke = Color.FromArgb("#2E3140");
                    ChipLabel.TextColor = Color.FromArgb("#B2B6CA");
                    break;

                default:
                    ChipBorder.BackgroundColor = Color.FromArgb("#232532");
                    ChipBorder.Stroke = Colors.Transparent;
                    ChipLabel.TextColor = Color.FromArgb("#CFD3E5");
                    break;
            }
        }
    }
}
