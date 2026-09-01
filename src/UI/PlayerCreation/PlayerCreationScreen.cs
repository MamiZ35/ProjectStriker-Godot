using Godot;
using Striker.Core.Career;
using Striker.Game;
using Striker.Gameplay;

namespace Striker.UI.PlayerCreation
{
    /// <summary>
    /// Sprint 9 karakter yaratma ekranı (Godot taşıması; DH-001/DEC-017):
    /// futbolcuya editable bir ad (öneri havuzundan ön-dolu; YENİ AD yeniden
    /// üretir) ve dört kimlik rolünden biri — Golcü / Oyun Kurucu / Kanat /
    /// Defans; yalnızca kelimeler, sıfır istatistik (DEC-012). Tek-seçim
    /// semantiği (D-007): seçili kart altın etiket + chevron + tam alfa
    /// taşır, diğer kartlar ~%45'e çöker. CTA ("KARİYERİNE BAŞLA") rol
    /// seçilene dek devre dışıdır; ad her zaman geçerlidir çünkü önden doludur.
    /// CTA ContractBeat sahnesini yükler (DEC-010). Sahne yapıyı taşır
    /// (scenes/PlayerCreation.tscn), renkleri bu sınıf uygular.
    /// </summary>
    public partial class PlayerCreationScreen : Control
    {
        private const string CardTexturePath = "res://assets/ui/Card_9slice_144.png";
        private const string ContractBeatScenePath = "res://scenes/ContractBeat.tscn";
        private const string DefaultOverTitle = "Y E N İ   K A R İ Y E R";

        private const float DisabledOpacity = 0.45f;
        private const int NameCharacterLimit = 24;

        private static readonly Color AccentGold = new(0.788235f, 0.658824f, 0.298039f, 1f);
        private static readonly Color LabelWhite = new(0.92f, 0.92f, 0.92f, 1f);

        private int _selectedRoleIndex = -1;

        private LineEdit _nameInput = null!;
        private Button _startButton = null!;
        private Button[] _roleCards = System.Array.Empty<Button>();
        private Label[] _roleLabels = System.Array.Empty<Label>();
        private TextureRect[] _roleChevrons = System.Array.Empty<TextureRect>();

        /// <summary>Ad girişi (test ve ileride kimlik üretimi için okunur).</summary>
        public LineEdit NameInput => _nameInput;

        /// <summary>Öneri adı yeniden üreten karıştırma düğmesi.</summary>
        public Button ShuffleButton => GetNode<Button>("NameSection/ShuffleButton");

        /// <summary>"KARİYERİNE BAŞLA" CTA'sı.</summary>
        public Button StartButton => _startButton;

        /// <summary>Rol kartları — sahne sırasıyla PlayerCreationContent.Roles.</summary>
        public Button[] RoleCards => _roleCards;

        /// <summary>Seçili rolün kart indeksi; hiçbir seçim yoksa -1.</summary>
        public int SelectedRoleIndex => _selectedRoleIndex;

        /// <summary>Seçili rol (enum); hiçbir seçim yoksa null.</summary>
        public PlayerRole? SelectedRole =>
            _selectedRoleIndex >= 0 ? PlayerCreationContent.Roles[_selectedRoleIndex] : null;

        /// <summary>Bir rol seçildi mi?</summary>
        public bool HasSelection => _selectedRoleIndex >= 0;

        public override void _Ready()
        {
            _nameInput = GetNode<LineEdit>("NameSection/NameInput");
            _nameInput.MaxLength = NameCharacterLimit;

            _startButton = GetNode<Button>("StartCard");
            _startButton.Pressed += OnStartClicked;

            var shuffleButton = GetNode<Button>("NameSection/ShuffleButton");
            shuffleButton.Pressed += OnShuffleClicked;

            WireRoleCards();
            NormalizeTexts();
            ApplyCardStyleboxes();
            ApplySelectionVisuals();
        }

        /// <summary>Shuffle: özgün IP öneri havuzundan yeni bir ad üretip girer.</summary>
        private void OnShuffleClicked()
        {
            _nameInput.Text = PlayerCreationContent.RandomName();
        }

        /// <summary>
        /// CTA: rol seçiliyse seçme gününe (ContractBeat) geçer. Faz 7: oyuncunun
        /// kimliği (ad + rol) bu noktada diske kalıcı olarak yazılır — böylece
        /// uygulama yeniden açılınca CONTINUE bu kariyere bağlanır.
        /// </summary>
        private void OnStartClicked()
        {
            if (!HasSelection)
            {
                return;
            }

            string name = _nameInput.Text.Trim();
            CareerSaveRepository.BeginCareer(name, SelectedRole!.Value);

            GetTree().ChangeSceneToFile(ContractBeatScenePath);
        }

        /// <summary>Tek-seçim: verilen kartı seçer, görselleri uygular, CTA'yı açar.</summary>
        private void SelectRole(int index)
        {
            if (_roleCards.Length == 0 || index < 0 || index >= _roleCards.Length)
            {
                return;
            }

            _selectedRoleIndex = index;
            ApplySelectionVisuals();

            _startButton.Disabled = false;
            _startButton.Modulate = new Color(1f, 1f, 1f, 1f);
        }

        private void WireRoleCards()
        {
            _roleCards = new Button[PlayerCreationContent.Roles.Count];
            _roleLabels = new Label[PlayerCreationContent.Roles.Count];
            _roleChevrons = new TextureRect[PlayerCreationContent.Roles.Count];

            for (int i = 0; i < PlayerCreationContent.Roles.Count; i++)
            {
                PlayerRole role = PlayerCreationContent.Roles[i];
                Button? card = GetNodeOrNull<Button>($"RoleSection/RoleStack/RoleCard_{i}");
                if (card == null)
                {
                    GD.PushError($"[PlayerCreationScreen] Rol kartı bulunamadı: RoleCard_{i}.");
                    continue;
                }

                card.GetNode<Label>("Label").Text = PlayerCreationContent.GetRoleLabel(role);
                card.GetNode<Label>("Sentence").Text = PlayerCreationContent.GetRoleSentence(role);

                int captured = i;
                card.Pressed += () => SelectRole(captured);
                _roleCards[i] = card;
                _roleLabels[i] = card.GetNode<Label>("Label");
                _roleChevrons[i] = card.GetNode<TextureRect>("Chevron");
            }
        }

        private void NormalizeTexts()
        {
            GetNode<Label>("OverTitle").Text = DefaultOverTitle;

            if (string.IsNullOrWhiteSpace(_nameInput.Text))
            {
                _nameInput.Text = PlayerCreationContent.RandomName();
            }
        }

        /// <summary>
        /// Seçim görseli (D-007): seçili kartın etiketi altına döner, chevron'u
        /// görünür olur ve kart tam alfa taşır; diğer kartlar ~%45'e çözülür.
        /// Hiçbir seçim yokken tüm kartlar tam alfadadır.
        /// </summary>
        private void ApplySelectionVisuals()
        {
            for (int i = 0; i < _roleCards.Length; i++)
            {
                if (_roleCards[i] == null || _roleLabels[i] == null || _roleChevrons[i] == null)
                {
                    continue;
                }

                bool selected = i == _selectedRoleIndex;

                _roleLabels[i].AddThemeColorOverride(
                    "font_color", selected ? AccentGold : LabelWhite);
                _roleChevrons[i].Visible = selected;
                _roleCards[i].Modulate =
                    !HasSelection || selected ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, DisabledOpacity);
            }

            if (!HasSelection)
            {
                _startButton.Disabled = true;
                _startButton.Modulate = new Color(1f, 1f, 1f, DisabledOpacity);
            }
        }

        private void ApplyCardStyleboxes()
        {
            Texture2D? cardTexture = ResourceLoader.Load<Texture2D>(CardTexturePath);
            if (cardTexture == null)
            {
                GD.PushError($"[PlayerCreationScreen] Kart dokusu bulunamadı: {CardTexturePath}.");
                return;
            }

            StyleBoxTexture cardStylebox = new()
            {
                Texture = cardTexture,
                TextureMarginLeft = 24f,
                TextureMarginRight = 24f,
                TextureMarginTop = 24f,
                TextureMarginBottom = 24f,
            };

            for (int i = 0; i < _roleCards.Length; i++)
            {
                Button? card = _roleCards[i];
                if (card == null)
                {
                    continue;
                }

                card.AddThemeStyleboxOverride("normal", cardStylebox);
                card.AddThemeStyleboxOverride("hover", cardStylebox);
                card.AddThemeStyleboxOverride("pressed", cardStylebox);
                card.AddThemeStyleboxOverride("focus", cardStylebox);
            }

            GetNode<Button>("NameSection/ShuffleButton")
                .AddThemeStyleboxOverride("normal", cardStylebox);
            _startButton.AddThemeStyleboxOverride("normal", cardStylebox);
            _startButton.AddThemeStyleboxOverride("hover", cardStylebox);
            _startButton.AddThemeStyleboxOverride("pressed", cardStylebox);

            StyleBoxFlat inputStylebox = new()
            {
                BgColor = new Color(0.05f, 0.05f, 0.07f, 0.7f),
                BorderColor = new Color(0.788235f, 0.658824f, 0.298039f, 0.6f),
                BorderWidthLeft = 1,
                BorderWidthRight = 1,
                BorderWidthTop = 1,
                BorderWidthBottom = 1,
            };

            _nameInput.AddThemeStyleboxOverride("normal", inputStylebox);
            _nameInput.AddThemeStyleboxOverride("focus", inputStylebox);
            _nameInput.AddThemeFontSizeOverride("font_size", 30);
            _nameInput.AddThemeColorOverride("font_color", LabelWhite);
        }
    }
}
