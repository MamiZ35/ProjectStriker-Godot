using System;
using System.Collections.Generic;
using Striker.Core.Career;

namespace Striker.Gameplay
{
    /// <summary>
    /// Sprint 9 karakter yaratmasının yazarak (authoring) içeriği. Hepsi özgün
    /// IP'dir — uydurma Türkçe ad havuzları; gerçek oyuncu adı yoktur (DEC-001).
    /// Rol verisi yalnızca kelimedir: etiket + tek satırlık kimlik cümlesi;
    /// hiçbir sayı/nitelik gösterilmez (DEC-012). Bu içerik bu sprintte hiçbir
    /// sistem tarafından tüketilmez — sonuçlar v1'de yalnızca anlatıdır (DH-001).
    /// </summary>
    public static class PlayerCreationContent
    {
        /// <summary>Özgün IP Türkçe erkek adı havuzu (uydurma; gerçek futbolcu adı değildir).</summary>
        private static readonly string[] FirstNames =
        {
            "Alaz",
            "Doruk",
            "Egehan",
            "Poyraz",
            "Rüzgar",
            "Yaman",
            "Atlas",
            "Kuzey",
            "Dağhan",
            "Tunç",
            "Bora",
            "Efe",
            "Arel",
            "Deren",
            "Keremcan",
            "Toprak",
        };

        /// <summary>Özgün IP Türkçe soyadı havuzu (uydurma; gerçek futbolcu soyadı değildir).</summary>
        private static readonly string[] Surnames =
        {
            "Karasu",
            "Bozkır",
            "Yamaç",
            "Alaca",
            "Poyrazoğlu",
            "Dumanlı",
            "Karayel",
            "Gündoğdu",
            "Taşdelen",
            "Işıltar",
            "Sarpdağ",
            "Gölgeç",
            "Batısoy",
            "Akçora",
            "Derbent",
            "Yağız",
        };

        /// <summary>
        /// Ad + soyad birleştirerek özgün bir öneri adı üretir. Anlatı amaçlıdır;
        /// deterministik gerekmediğinden <c>System.Random</c> kullanılır.
        /// </summary>
        private static readonly System.Random Rng = new();

        public static string RandomName()
        {
            string first = FirstNames[Rng.Next(0, FirstNames.Length)];
            string surname = Surnames[Rng.Next(0, Surnames.Length)];
            return first + " " + surname;
        }

        /// <summary>Rolün görünen Türkçe etiketini döndürür.</summary>
        public static string GetRoleLabel(PlayerRole role)
        {
            switch (role)
            {
                case PlayerRole.Striker: return "Golcü";
                case PlayerRole.Playmaker: return "Oyun Kurucu";
                case PlayerRole.Winger: return "Kanat";
                case PlayerRole.Defender: return "Defans";
                default: throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        /// <summary>Rolün PO-onaylı tek satırlık kimlik cümlesini birebir döndürür.</summary>
        public static string GetRoleSentence(PlayerRole role)
        {
            switch (role)
            {
                case PlayerRole.Striker: return "Kaleye bakar, gerisini düşünmez.";
                case PlayerRole.Playmaker: return "Oyun onun avucunda durur.";
                case PlayerRole.Winger: return "Hızın ve cesaretin adı.";
                case PlayerRole.Defender: return "Sessiz duvar. Kimse geçemez.";
                default: throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        /// <summary>Kart sırasını sahne üretimiyle paylaşan sabit rol dizisi (tek seçim kaynağı).</summary>
        public static IReadOnlyList<PlayerRole> Roles { get; } = new[]
        {
            PlayerRole.Striker,
            PlayerRole.Playmaker,
            PlayerRole.Winger,
            PlayerRole.Defender,
        };
    }
}
