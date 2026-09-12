using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace MauiTrainApp.Controls
{
    public partial class SectionHeader : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(SectionHeader), string.Empty);

        public static readonly BindableProperty ActionTextProperty =
            BindableProperty.Create(nameof(ActionText), typeof(string), typeof(SectionHeader), string.Empty, propertyChanged: OnActionTextChanged);

        public static readonly BindableProperty ActionCommandProperty =
            BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(SectionHeader));

        public static readonly BindableProperty HasActionProperty =
            BindableProperty.Create(nameof(HasAction), typeof(bool), typeof(SectionHeader), false);

        public SectionHeader()
        {
            InitializeComponent();
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string ActionText
        {
            get => (string)GetValue(ActionTextProperty);
            set => SetValue(ActionTextProperty, value);
        }

        public ICommand? ActionCommand
        {
            get => (ICommand?)GetValue(ActionCommandProperty);
            set => SetValue(ActionCommandProperty, value);
        }

        public bool HasAction
        {
            get => (bool)GetValue(HasActionProperty);
            private set => SetValue(HasActionProperty, value);
        }

        private static void OnActionTextChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SectionHeader header)
            {
                header.HasAction = !string.IsNullOrWhiteSpace(newValue as string);
            }
        }
    }
}
