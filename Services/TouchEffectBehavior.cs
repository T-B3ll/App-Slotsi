using Microsoft.Maui.Controls;

namespace slotsi_citas.Services;

public class TouchEffectBehavior : Behavior<View>
{
    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);

        // Usamos TapGestureRecognizer para compatibilidad completa en Android
        var tapGesture = new TapGestureRecognizer();

        tapGesture.Tapped += async (s, e) =>
        {
            if (bindable != null)
            {
                // Animación rápida de opacidad al pulsar
                await bindable.FadeTo(0.4, 80, Easing.CubicOut);
                await bindable.FadeTo(1.0, 100, Easing.CubicIn);
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