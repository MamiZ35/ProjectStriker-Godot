using Godot;

namespace Striker.UI.Shared
{
    /// <summary>
    /// Saf alan (SafeArea) uygulayıcısı: ekranın güvenli bölgesini (notch/kamera
    /// deliği/köşe kesitleri) sahne KÖKÜNE (parent Control) anchor olarak uygular;
    /// menü içeriği güvenli bölgede kalır (Faz 5, Android cutout).
    ///
    /// DisplayServer.ScreenGetUsableRect yalnızca Android/iOS'ta gerçek değer
    /// döndürür; diğer platformlarda (desktop/headless) tam ekran fallback'i
    /// geldiği için bu orada no-op kalır. Değerler normalize oran olduğu için
    /// 720×1560 canvas_items stretch alanında doğru çalışır. Godot yön/ekran
    /// değişiminde callback sunmadığından _Process içinde değişiklik kontrolü
    /// yapılır — maliyet düşüktür.
    ///
    /// Sahne içinde kökün ilk çocuğu olarak durur; kendisi hiçbir şey çizmez.
    /// </summary>
    public partial class SafeArea : Control
    {
        private const int Screen = (int)DisplayServer.ScreenOfMainWindow;

        private Rect2I _appliedSafeArea = default;
        private bool _hasApplied;

        /// <summary>En son köke uygulanan safeArea (tanı/hudlama için).</summary>
        public Rect2I AppliedSafeArea => _appliedSafeArea;

        public override void _Ready()
        {
            Apply();
        }

        public override void _Process(double delta)
        {
            Apply();
        }

        private void Apply()
        {
            Control? root = GetParent() as Control;
            if (root == null)
            {
                return;
            }

            Vector2I screenSize = DisplayServer.ScreenGetSize(Screen);
            if (screenSize.X <= 0 || screenSize.Y <= 0)
            {
                return;
            }

            Rect2I safeArea = DisplayServer.ScreenGetUsableRect(Screen);
            if (safeArea.Size.X <= 0 || safeArea.Size.Y <= 0)
            {
                // Güvenli bölge bilgi yok: tam ekran kabul et.
                safeArea = new Rect2I(0, 0, screenSize.X, screenSize.Y);
            }

            if (_hasApplied && safeArea == _appliedSafeArea)
            {
                return;
            }

            _hasApplied = true;
            _appliedSafeArea = safeArea;

            root.AnchorLeft = safeArea.Position.X / (float)screenSize.X;
            root.AnchorTop = safeArea.Position.Y / (float)screenSize.Y;
            root.AnchorRight = (safeArea.Position.X + safeArea.Size.X) / (float)screenSize.X;
            root.AnchorBottom = (safeArea.Position.Y + safeArea.Size.Y) / (float)screenSize.Y;
            root.OffsetLeft = 0f;
            root.OffsetTop = 0f;
            root.OffsetRight = 0f;
            root.OffsetBottom = 0f;
            root.GrowHorizontal = Control.GrowDirection.Begin | Control.GrowDirection.End;
            root.GrowVertical = Control.GrowDirection.Begin | Control.GrowDirection.End;
        }
    }
}
