using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace MauiTrainApp.Views.Base
{
    public abstract class StubPageBase : ContentPage
    {
        private const string Note = "Экран собирается на следующих фазах плана";

        private readonly VerticalStackLayout _layout;

        protected StubPageBase(string caption)
        {
            Title = caption;

            _layout = new VerticalStackLayout
            {
                Padding = new Thickness(17),
                Spacing = 8
            };

            _layout.Add(new Label
            {
                Text = caption,
                FontSize = 24,
                FontAttributes = FontAttributes.Bold
            });

            _layout.Add(new Label
            {
                Text = Note,
                FontSize = 12,
                TextColor = Color.FromArgb("#9397AB")
            });

            Content = new ScrollView { Content = _layout };
        }

        protected void AddLink(string text, Func<Task> navigate)
        {
            var button = new Button
            {
                Text = text,
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#E9E9ED"),
                BorderColor = Color.FromArgb("#2E3140"),
                BorderWidth = 1,
                CornerRadius = 10,
                HeightRequest = 46
            };

            button.Clicked += async (_, _) => await navigate();

            _layout.Add(button);
        }
    }
}
