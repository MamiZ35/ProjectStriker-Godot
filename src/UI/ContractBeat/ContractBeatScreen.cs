using Godot;
using Striker.Core.Career;
using Striker.Gameplay.ContractBeat;
using Striker.UI.Shared;

namespace Striker.UI.ContractBeat
{
    /// <summary>
    /// Contract beat dilimini dikey (portrait), dokunma-öncelikli bir tam ekran
    /// arayüzle oyuncuya sunar. Ekran yalnızca scenes/ContractBeat.tscn üzerinden
    /// yüklenir (Godot taşıması: UI artık sahne dosyasıyla, SceneBuilder ile değil).
    /// </summary>
    public partial class ContractBeatScreen : Control
    {
        private const string BoldFontPath = "res://assets/fonts/NotoSans-Bold.ttf";

        private static readonly Color ButtonColor = new(0.15f, 0.36f, 0.30f, 1f);
        private static readonly Color ButtonHoverColor = new(0.19f, 0.44f, 0.36f, 1f);
        private static readonly Color ButtonPressedColor = new(0.12f, 0.30f, 0.25f, 1f);
        private static readonly Color TextColor = new(0.92f, 0.92f, 0.92f, 1f);

        private ContractBeatViewModel? _viewModel = null;
        private Label? _titleText = null;
        private Label? _bodyText = null;
        private VBoxContainer? _optionsRoot = null;

        public override void _Ready()
        {
            _viewModel = new ContractBeatViewModel(ContractBeatContent.BuildFlow());
            _titleText = GetNode<Label>("Title");
            _bodyText = GetNode<Label>("Body");
            _optionsRoot = GetNode<VBoxContainer>("Options");

            Render();
        }

        private void Render()
        {
            _titleText!.Text = _viewModel!.Title;
            _bodyText!.Text = _viewModel.BodyText;

            ClearOptions();

            for (int i = 0; i < _viewModel.Options.Count; i++)
            {
                BeatChoice choice = _viewModel.Options[i];
                CreateOptionButton(choice);
            }

            // Sprint 10 (DEC-018): yalnızca İMZALANMIŞ sonunda ilk antrenmana
            // geçiş affordansı. Kendi handler'ındadır — HandleOption/Choose
            // çözümlenmiş akışta InvalidOperationException fırlatır.
            if (_viewModel.IsSignedResolution)
            {
                CreateTrainingCampNavigation();
            }
        }

        private void HandleOption(BeatChoice choice)
        {
            if (_viewModel!.IsOnOfferBeat)
            {
                _viewModel.Decide(choice.Id == "accept");
            }
            else
            {
                _viewModel.Choose(choice.Id);
            }

            Render();
        }

        private void CreateOptionButton(BeatChoice choice)
        {
            Button button = CreateButton(choice.Label, boldLabel: false);
            button.Pressed += () => HandleOption(choice);
            _optionsRoot!.AddChild(button);
        }

        /// <summary>
        /// "İLK ANTRENMAN" geçiş düğmesi: imzalı sondan TrainingCamp sahnesine
        /// geçirir. BeatChoice DEĞİLDİR — akışa Choose/Decide çağrısı yapmaz.
        /// </summary>
        private void CreateTrainingCampNavigation()
        {
            Button button = CreateButton("İLK ANTRENMAN", boldLabel: true);
            button.Pressed += () => GetTree().ChangeSceneToFile("res://scenes/TrainingCamp.tscn");
            _optionsRoot!.AddChild(button);
        }

        private Button CreateButton(string label, bool boldLabel)
        {
            Button button = new();
            button.Text = label;
            button.CustomMinimumSize = new Vector2(0f, 88f);
            // Faz 5: basış geri bildirimi (dinamik butonlar için sahne script'i yok).
            button.ButtonDown += () => PressFeedback.Pulse(button);
            button.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
            button.AddThemeFontSizeOverride("font_size", 28);

            if (boldLabel)
            {
                button.AddThemeFontOverride("font", ResourceLoader.Load<FontFile>(BoldFontPath));
            }

            button.AddThemeColorOverride("font_color", TextColor);
            button.AddThemeColorOverride("font_hover_color", new Color(1f, 1f, 1f, 1f));
            button.AddThemeColorOverride("font_pressed_color", new Color(0.85f, 0.85f, 0.85f, 1f));

            button.AddThemeStyleboxOverride("normal", new StyleBoxFlat { BgColor = ButtonColor });
            button.AddThemeStyleboxOverride("hover", new StyleBoxFlat { BgColor = ButtonHoverColor });
            button.AddThemeStyleboxOverride("pressed", new StyleBoxFlat { BgColor = ButtonPressedColor });
            button.AddThemeStyleboxOverride("focus", new StyleBoxFlat { BgColor = ButtonHoverColor });

            return button;
        }

        private void ClearOptions()
        {
            foreach (Node child in _optionsRoot!.GetChildren())
            {
                _optionsRoot.RemoveChild(child);
                child.QueueFree();
            }
        }
    }
}
