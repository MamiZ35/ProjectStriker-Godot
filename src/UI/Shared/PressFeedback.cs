using Godot;

namespace Striker.UI.Shared
{
    /// <summary>
    /// Basış geri bildirimi (Sprint 8B basış dili): bağlı kart 120 ms boyunca
    /// ~%97'ye inip geri gelir (0.97/120 ms). Sahne butonlarına script olarak
    /// bağlanır (Button türevi); devre dışı butonlarda (kilitli CTA) dalgalanmaz.
    ///
    /// Dinamik/programatik üretilen butonlar (örn. ContractBeat option'ları)
    /// için: button.ButtonDown += (Button b) => PressFeedback.Pulse(b);
    /// </summary>
    public partial class PressFeedback : Button
    {
        private const float ScaleTarget = 0.97f;
        private const float DurationSeconds = 0.12f;

        private const string TweenMetaName = "press_tween";

        public override void _Ready()
        {
            ButtonDown += HandleButtonDown;
        }

        public override void _ExitTree()
        {
            ButtonDown -= HandleButtonDown;
            KillPressTween();
            Scale = Vector2.One;
        }

        private void HandleButtonDown()
        {
            Pulse(this);
        }

        /// <summary>
        /// Tek basış animasyonu: 60 ms ease-out (out-quad) iniş + 60 ms ease-in
        /// (in-sine) dönüş. Hızlı çift basışta önceki tween öldürülür.
        /// </summary>
        public static void Pulse(Button button)
        {
            if (button == null || button.Disabled || button.GetTree() == null)
            {
                return;
            }

            KillButtonTween(button);

            // Ölçek merkeze göre: pivot'u orta noktalara al.
            button.PivotOffset = button.Size / 2f;

            float halfDuration = DurationSeconds * 0.5f;
            Tween tween = button.GetTree().CreateTween();
            tween.TweenProperty(button, "scale", new Vector2(ScaleTarget, ScaleTarget), halfDuration)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
            tween.TweenProperty(button, "scale", Vector2.One, halfDuration)
                .SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);

            button.SetMeta(TweenMetaName, tween);
        }

        private static void KillButtonTween(Button button)
        {
            if (!button.HasMeta(TweenMetaName))
            {
                return;
            }

            if (button.GetMeta(TweenMetaName).As<Tween>() is { } tween && tween.IsValid())
            {
                tween.Kill();
            }

            button.RemoveMeta(TweenMetaName);
        }

        private void KillPressTween()
        {
            KillButtonTween(this);
        }
    }
}
