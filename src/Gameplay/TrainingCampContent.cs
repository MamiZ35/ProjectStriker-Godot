namespace Striker.Gameplay
{
    /// <summary>
    /// Sprint 10 "İlk Antrenman" anlatı içeriği (docs/12_FIRST_30_MINUTES.md
    /// adım 5 — hissi: büyüme). Motor-bağımsızdır. Tümü özgün IP Türkçe metin;
    /// KARAKTER HAVUZU DEĞİLDİR — antrenör isimsiz bir anlatı aygıtıdır (DH-002).
    /// Sıfır rakam, sıfır takvim/season çerçevesi, sıfır gelişim ekseni: seçim
    /// yalnızca kapanış cümlesinin TONUNU değiştirir (ContractBeat seçim→ton
    /// dilbilgisi), hiçbir yeteneği işaret etmez (DEC-012, DEC-018).
    /// </summary>
    public static class TrainingCampContent
    {
        /// <summary>Üst-başlık — harf aralıklı altın satır (Sprint 9 dili).</summary>
        public const string OverTitle = "İ L K   A N T R E N M A N";

        /// <summary>Tek satırlık çerçeve cümlesi.</summary>
        public const string TitleLine = "Burası senin kulübün artık.";

        /// <summary>Antrenör karşılaması — isimsiz; kimlik verisi tüketmez (D-104).</summary>
        public const string CoachWelcome =
            "Antrenör sahnenin kenarında seni karşılar: burada kimse sana bir şey "
            + "hediye etmez. Her futbolcu bu çimende kendini bir kez daha kurar.";

        /// <summary>Seçim bölümünün etiketi.</summary>
        public const string ChoiceSectionLabel = "KENDİNİ NASIL GÖSTERİRSİN?";

        /// <summary>Ufuk kartı damgası — zaman vaadi değil, sıradaki adım.</summary>
        public const string HorizonEyebrow = "SIRADA";

        /// <summary>Ufuk kartı başlığı (docs/12 adım 6 çekimi).</summary>
        public const string HorizonTitle = "İLK MAÇ";

        /// <summary>Ufuk kartı satırı.</summary>
        public const string HorizonLine = "Forman hazırlandı. Sıra sahada kanıtlamakta.";

        /// <summary>Yapım sonundaki küçük çıkış affordansı.</summary>
        public const string MenuAffordanceLabel = "MENÜ";

        /// <summary>"Sahnedekal" seçiminin kimliği.</summary>
        public const string StayChoiceId = "stay";

        /// <summary>"Soyunma odası" seçiminin kimliği.</summary>
        public const string RoomChoiceId = "room";

        /// <summary>Tek anlamlı seçim: iki ilk-gün tavrı. Yetenek ekseni DEĞİLDİR.</summary>
        public static readonly TrainingChoice[] Choices =
        {
            new TrainingChoice(
                StayChoiceId,
                "SAHNEDE KAL",
                "Sahne boşaldığında hâlâ sen oradasın.",
                "Tekralar bitmişti ki antrenör yanına geldi: \"İşte bu. Bu saha "
                + "çalışkanlara güler — sen kendini kanıtlamaya şimdiden başladın.\""),
            new TrainingChoice(
                RoomChoiceId,
                "SOYUNMA ODASI",
                "Kulübü dinleyen futbolcu, oyunu daha çabuk okur.",
                "Soyunma odasının seslerini dinledin: kıdemli kahkahalar, genç "
                + "sessizlik. Antrenör kapıdan girerken gülümsedi: \"İyi işaret. "
                + "Oyunu önce kulaklar öğrenir — sen doğru yerdesin.\""),
        };

        /// <summary>Seçime göre antrenörün kapanış cümlesi (tek yankı: ekranda ton).</summary>
        public static string GetClosing(string choiceId)
        {
            foreach (TrainingChoice choice in Choices)
            {
                if (choice.Id == choiceId)
                {
                    return choice.Closing;
                }
            }

            throw new System.ArgumentException($"Bilinmeyen seçim kimliği: '{choiceId}'.", nameof(choiceId));
        }
    }

    /// <summary>Bir ilk-gün tavri: kimlik + kart metni + o seçime özgü kapanış.</summary>
    public sealed class TrainingChoice
    {
        /// <summary>Kararlı kimlik (test ve kapanış eşlemesi için).</summary>
        public string Id { get; }

        /// <summary>Kart etiketi — büyük harf, kısa.</summary>
        public string Label { get; }

        /// <summary>Kartın tek satırlık his cümlesi.</summary>
        public string Line { get; }

        /// <summary>Antrenörün bu tavra özel kapanış cümlesi.</summary>
        public string Closing { get; }

        public TrainingChoice(string id, string label, string line, string closing)
        {
            Id = id;
            Label = label;
            Line = line;
            Closing = closing;
        }
    }
}
