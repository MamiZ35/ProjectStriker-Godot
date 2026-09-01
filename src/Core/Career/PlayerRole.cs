namespace Striker.Core.Career
{
    /// <summary>
    /// Sprint 9 karakter yaratmasındaki dört kimlik rolü. Yalnızca kelimelerdir —
    /// hiçbir istatistik, nitelik veya sayı taşımaz (DEC-012: hissedilen, sayılmayan).
    /// Türkçe etiket eşleşmesi: Striker = "Golcü", Playmaker = "Oyun Kurucu",
    /// Winger = "Kanat", Defender = "Defans" (görünen metinler
    /// <c>PlayerCreationContent</c> üzerinden sağlanır).
    /// </summary>
    public enum PlayerRole
    {
        /// <summary>Golcü — kaleye bakar, gerisini düşünmez.</summary>
        Striker = 0,

        /// <summary>Oyun Kurucu — oyun onun avucunda durur.</summary>
        Playmaker = 1,

        /// <summary>Kanat — hızın ve cesaretin adı.</summary>
        Winger = 2,

        /// <summary>Defans — sessiz duvar; kimse geçemez.</summary>
        Defender = 3,
    }
}
