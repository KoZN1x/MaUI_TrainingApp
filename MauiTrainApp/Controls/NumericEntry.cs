using Microsoft.Maui.Controls;

namespace MauiTrainApp.Controls
{
    public sealed class NumericEntry : Entry
    {
        public NumericEntry()
        {
            Keyboard = Keyboard.Numeric;
        }
    }
}
