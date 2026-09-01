using Godot;

namespace Striker.UI.MainMenu
{
    /// <summary>
    /// Sprint 8B ana menü ekranı (Godot taşıması; D-005). CTA yığında tam olarak
    /// ÜÇ tam genişlik cam kart vardır; yalnızca NEW CAREER aktiftir. CONTINUE ve
    /// SETTINGS görünür ama devre dışıdır (kayıt sistemi / backend henüz yok).
    /// NEWS yalnızca üst-sol hexagon rozeti olarak bulunur (dekoratif, devre
    /// dışı; kartı yoktur). Yer tutucu modal ya da "Coming Soon" metni YOKTUR.
    /// Görsel stil bu sınıf üzerinden uygulanır: sahne yapıyı taşır, renkleri değil.
    /// </summary>
    public partial class MainMenuScreen : Control
    {
        private const string CardTexturePath = "res://assets/ui/Card_9slice_144.png";
        private const string PlayerCreationScenePath = "res://scenes/PlayerCreation.tscn";

        /// <summary>Devre dışı kartların/rozetlerin bütünleşik solukluk seviyesi (~%45).</summary>
        private const float DisabledOpacity = 0.45f;

        private const string DefaultTitle = "P R O J E C T   S T R I K E R";
        private const string DefaultTagline = "YOUR CAREER. YOUR LEGACY.";
        private const string DefaultNewsLabel = "NEWS";
        private const string DefaultFooterVersion = "v1.0.0";
        private const string DefaultFooterCopyright = "© 2026 Striker Studios";

        private Button _newCareerButton = null!;
        private Button _continueButton = null!;
        private Button _settingsButton = null!;

        public Button NewCareerButton => _newCareerButton;
        public Button ContinueButton => _continueButton;
        public Button SettingsButton => _settingsButton;

        public override void _Ready()
        {
            _newCareerButton = GetNode<Button>("CTAStack/NewCareerCard");
            _continueButton = GetNode<Button>("CTAStack/ContinueCard");
            _settingsButton = GetNode<Button>("CTAStack/SettingsCard");

            _newCareerButton.Pressed += OnNewCareerClicked;

            ApplyStyling();
            ApplyCardStyleboxes();
            NormalizeTexts();
        }

        /// <summary>
        /// NEW CAREER: karakter yaratma ekranına (PlayerCreation) geçirir —
        /// creation, seçme gününden önce durur (DH-001/DEC-017).
        /// </summary>
        private void OnNewCareerClicked()
        {
            GetTree().ChangeSceneToFile(PlayerCreationScenePath);
        }

        private void ApplyStyling()
        {
            StyleCard(_newCareerButton, interactable: true);
            StyleCard(_continueButton, interactable: false);
            StyleCard(_settingsButton, interactable: false);
            StyleNewsChip();
        }

        private void StyleCard(Button button, bool interactable)
        {
            button.Disabled = !interactable;
            button.Modulate =
                interactable ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, DisabledOpacity);
        }

        private void ApplyCardStyleboxes()
        {
            Texture2D? cardTexture = ResourceLoader.Load<Texture2D>(CardTexturePath);
            if (cardTexture == null)
            {
                GD.PushError("[MainMenuScreen] Kart dokusu bulunamadı.");
                return;
            }

            // 9-slice köşe: kartın yuvarlatılmış köşesi ~12px'te başlar; 24px güvenli bölge.
            StyleBoxTexture cardStylebox = new()
            {
                Texture = cardTexture,
                TextureMarginLeft = 24f,
                TextureMarginRight = 24f,
                TextureMarginTop = 24f,
                TextureMarginBottom = 24f,
            };

            ApplyCardStylebox(_newCareerButton, cardStylebox);
            ApplyCardStylebox(_continueButton, cardStylebox);
            ApplyCardStylebox(_settingsButton, cardStylebox);
        }

        private void ApplyCardStylebox(Button button, StyleBoxTexture stylebox)
        {
            // Unity'de Transition.None — Godot'ta da tüm durumlar aynı texture (Faz 5: PressFeedback).
            button.AddThemeStyleboxOverride("normal", stylebox);
            button.AddThemeStyleboxOverride("hover", stylebox);
            button.AddThemeStyleboxOverride("pressed", stylebox);
            button.AddThemeStyleboxOverride("focus", stylebox);
        }

        private void StyleNewsChip()
        {
            Control? chip = GetNodeOrNull<Control>("TopBar/NewsChip");
            if (chip != null)
            {
                chip.Modulate = new Color(1f, 1f, 1f, DisabledOpacity);
                chip.MouseFilter = Control.MouseFilterEnum.Ignore;
            }
        }

        private void NormalizeTexts()
        {
            SetFallbackText(GetNode<Label>("TitleBlock/Title"), DefaultTitle);
            SetFallbackText(GetNode<Label>("TitleBlock/Tagline"), DefaultTagline);
            SetFallbackText(GetNode<Label>("TopBar/NewsChip/Label"), DefaultNewsLabel);
            SetFallbackText(GetNode<Label>("Footer/Version"), DefaultFooterVersion);
            SetFallbackText(GetNode<Label>("Footer/Copyright"), DefaultFooterCopyright);
        }

        private static void SetFallbackText(Label label, string fallback)
        {
            if (label != null && string.IsNullOrWhiteSpace(label.Text))
            {
                label.Text = fallback;
            }
        }
    }
}
