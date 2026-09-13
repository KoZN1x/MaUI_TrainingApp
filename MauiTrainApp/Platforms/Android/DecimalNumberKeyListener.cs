using Android.Text;
using Android.Text.Method;

namespace MauiTrainApp
{
    internal sealed class DecimalNumberKeyListener : NumberKeyListener
    {
        private static readonly char[] Accepted = "0123456789.,".ToCharArray();

        public static DecimalNumberKeyListener Instance { get; } = new();

        public override InputTypes InputType => InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;

        protected override char[] GetAcceptedChars() => Accepted;
    }
}
