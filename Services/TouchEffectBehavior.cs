using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace slotsi_citas.Services
{
    public class TouchEffectBehavior : Behavior<View>
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(TouchEffectBehavior), null);

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(TouchEffectBehavior), null);

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            var tapGesture = new TapGestureRecognizer();

            tapGesture.Tapped += async (s, e) =>
            {
                if (bindable != null)
                {
                    // Animación visual de toque
                    await bindable.FadeTo(0.5, 70, Easing.CubicOut);
                    await bindable.FadeTo(1.0, 90, Easing.CubicIn);

                    // Ejecutar el comando del ViewModel si está asignado
                    if (Command != null && Command.CanExecute(CommandParameter))
                    {
                        Command.Execute(CommandParameter);
                    }
                }
            };

            bindable.GestureRecognizers.Add(tapGesture);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.GestureRecognizers.Clear();
        }
    }
}