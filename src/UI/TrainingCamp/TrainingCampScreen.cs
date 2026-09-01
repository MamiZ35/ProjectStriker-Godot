using Godot;
using Striker.Gameplay;

namespace Striker.UI.TrainingCamp
{
    /// <summary>
    /// Sprint 10 "İlk Antrenman" ekranı (Godot taşıması; docs/12 adım 5 —
    /// hissi: büyüme). İki beat'li anlatı mikro-akışı: antrenör karşılama +
    /// tek anlamlı seçim (SAHNEDE KAL / SOYUNMA ODASI), ardından seçimin
    /// tonladığı antrenör kapanışı ve etkileşimsiz "İLK MAÇ" ufuk kartı
    /// (D-105: Button DEĞİLDİR — asla açılmaz). Seçim yalnızca tonu
    /// değiştirir; hiçbir stat/yetenek işaretlemez (DEC-012, D-102).
    /// Sahne yapıyı taşır (scenes/TrainingCamp.tscn), renkleri ve
    /// stylebox'ları bu sınıf programatik uygular.
    /// </summary>
    public partial class TrainingCampScreen : Control
    {
        private const string CardTexturePath = "res://assets/ui/Card_9slice_144.png";
        private const string MainMenuScenePath = "res://scenes/MainMenu.tscn";

        private bool _choiceResolved;
        private Control? _choiceSection;
        private Label? _closingText;

        private Button[] _choiceCards = System.Array.Empty<Button>();

        /// <summary>Bir tavır seçildi mi? Seçimden sonra kapanış beat'i görünür.</summary>
        public bool ChoiceResolved => _choiceResolved;

        /// <summary>Seçimin tonladığı antrenör kapanış metni.</summary>
        public Label? ClosingText => _closingText;

        /// <summary>Seçim kartları — sahne sırasıyla TrainingCampContent.Choices.</summary>
        public Button[] ChoiceCards => _choiceCards;

        public override void _Ready()
        {
            _choiceSection = GetNode<Control>("ChoiceSection");
            _closingText = GetNode<Label>("ClosingRoot/ClosingCard/ClosingText");

            SetContentTexts();
            WireChoiceCards();

            Button? menuButton = GetNodeOrNull<Button>("ClosingRoot/MenuButton");
            if (menuButton == null)
            {
                GD.PushError("[TrainingCampScreen] 'ClosingRoot/MenuButton' bulunamadı.");
            }
            else
            {
                menuButton.Text = TrainingCampContent.MenuAffordanceLabel;
                menuButton.Pressed += OnMenuClicked;
            }

            ApplyCardStyleboxes(menuButton);
        }

        private void SetContentTexts()
        {
            GetNode<Label>("OverTitle").Text = TrainingCampContent.OverTitle;
            GetNode<Label>("TitleLine").Text = TrainingCampContent.TitleLine;
            GetNode<Label>("CoachCard/Welcome").Text = TrainingCampContent.CoachWelcome;
            GetNode<Label>("ChoiceSection/SectionLabel").Text = TrainingCampContent.ChoiceSectionLabel;
            GetNode<Label>("ClosingRoot/HorizonCard/HorizonEyebrow").Text = TrainingCampContent.HorizonEyebrow;
            GetNode<Label>("ClosingRoot/HorizonCard/HorizonTitle").Text = TrainingCampContent.HorizonTitle;
            GetNode<Label>("ClosingRoot/HorizonCard/HorizonLine").Text = TrainingCampContent.HorizonLine;
        }

        private void WireChoiceCards()
        {
            _choiceCards = new Button[TrainingCampContent.Choices.Length];

            for (int i = 0; i < TrainingCampContent.Choices.Length; i++)
            {
                TrainingChoice choice = TrainingCampContent.Choices[i];
                Button? card = GetNodeOrNull<Button>($"ChoiceSection/ChoiceStack/ChoiceCard_{i}");
                if (card == null)
                {
                    GD.PushError($"[TrainingCampScreen] Seçim kartı bulunamadı: ChoiceCard_{i}.");
                    continue;
                }

                card.GetNode<Label>("Label").Text = choice.Label;
                card.GetNode<Label>("Sentence").Text = choice.Line;

                string choiceId = choice.Id;
                card.Pressed += () => Choose(choiceId);
                _choiceCards[i] = card;
            }
        }

        /// <summary>Tek anlamlı seçim: kapanış beat'ini seçimin tonuyla açar.</summary>
        private void Choose(string choiceId)
        {
            if (_choiceResolved)
            {
                return;
            }

            _choiceResolved = true;
            _closingText!.Text = TrainingCampContent.GetClosing(choiceId);
            _choiceSection!.Visible = false;
            GetNode<Control>("ClosingRoot").Visible = true;
        }

        private void OnMenuClicked()
        {
            GetTree().ChangeSceneToFile(MainMenuScenePath);
        }

        private void ApplyCardStyleboxes(Button? menuButton)
        {
            Texture2D? cardTexture = ResourceLoader.Load<Texture2D>(CardTexturePath);
            if (cardTexture == null)
            {
                GD.PushError($"[TrainingCampScreen] Kart dokusu bulunamadı: {CardTexturePath}.");
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

            ApplyPanelStylebox("CoachCard", cardStylebox);
            ApplyPanelStylebox("ClosingRoot/ClosingCard", cardStylebox);
            ApplyPanelStylebox("ClosingRoot/HorizonCard", cardStylebox);

            for (int i = 0; i < _choiceCards.Length; i++)
            {
                Button? card = _choiceCards[i];
                if (card == null)
                {
                    continue;
                }

                // Unity'de Transition.None — Godot'ta da tüm durumlar aynı
                // texture (Faz 5: PressFeedback dokunma geri bildirimi).
                card.AddThemeStyleboxOverride("normal", cardStylebox);
                card.AddThemeStyleboxOverride("hover", cardStylebox);
                card.AddThemeStyleboxOverride("pressed", cardStylebox);
                card.AddThemeStyleboxOverride("focus", cardStylebox);
            }

            if (menuButton != null)
            {
                StyleBoxFlat emptyStylebox = new()
                {
                    BgColor = new Color(1f, 1f, 1f, 0f),
                };

                menuButton.AddThemeStyleboxOverride("normal", emptyStylebox);
                menuButton.AddThemeStyleboxOverride("hover", emptyStylebox);
                menuButton.AddThemeStyleboxOverride("pressed", emptyStylebox);
                menuButton.AddThemeStyleboxOverride("focus", emptyStylebox);
                menuButton.AddThemeFontSizeOverride("font_size", 18);
                menuButton.AddThemeColorOverride("font_color", new Color(0.92f, 0.92f, 0.92f, 0.45f));
            }
        }

        private void ApplyPanelStylebox(string path, StyleBoxTexture stylebox)
        {
            Panel? panel = GetNodeOrNull<Panel>(path);
            if (panel == null)
            {
                GD.PushError($"[TrainingCampScreen] Panel bulunamadı: {path}.");
                return;
            }

            panel.AddThemeStyleboxOverride("panel", stylebox);
        }
    }
}
