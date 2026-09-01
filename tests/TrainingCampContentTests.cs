using System;
using System.Linq;
using Striker.Gameplay;
using Xunit;

namespace Striker.Tests
{
    /// <summary>
    /// Sprint 10 "İlk Antrenman" içeriğini doğrular: iki seçim, seçime özgü
    /// kapanışlar ve en önemlisi — oyuncuya görünen HİÇBİR metinde sayı yoktur
    /// (DEC-012). Takvim/season çerçevesi de yasaktır (docs/13 §8 Q2 açık).
    /// </summary>
    public class TrainingCampContentTests
    {
        private static void AssertNoNumbers(params string[] texts)
        {
            foreach (string text in texts)
            {
                Assert.False(
                    System.Text.RegularExpressions.Regex.IsMatch(text, "[0-9]"),
                    "Oyuncunun göreceği metinde sayı olmamalı: \"" + text + "\"");
            }
        }

        private static string[] AllPlayerVisibleStrings() =>
            new[]
            {
                TrainingCampContent.OverTitle,
                TrainingCampContent.TitleLine,
                TrainingCampContent.CoachWelcome,
                TrainingCampContent.ChoiceSectionLabel,
                TrainingCampContent.HorizonEyebrow,
                TrainingCampContent.HorizonTitle,
                TrainingCampContent.HorizonLine,
                TrainingCampContent.MenuAffordanceLabel,
            }
            .Concat(TrainingCampContent.Choices.Select(choice => choice.Label))
            .Concat(TrainingCampContent.Choices.Select(choice => choice.Line))
            .Concat(TrainingCampContent.Choices.Select(choice => choice.Closing))
            .ToArray();

        [Fact]
        public void Icerik_TamIkiSecimTasir_VeKimliklerBenzersizdir()
        {
            Assert.Equal(2, TrainingCampContent.Choices.Length);
            Assert.Equal(2, TrainingCampContent.Choices.Select(choice => choice.Id).Distinct().Count());
        }

        [Fact]
        public void Icerik_HicbirGorunurMetindeSayiKullanmaz()
        {
            AssertNoNumbers(AllPlayerVisibleStrings());
        }

        [Fact]
        public void Icerik_TakvimCercevesiKullanmaz_SeasonSorusuAcikKalir()
        {
            // docs/13 §8 Q2 (sezon/zaman yapısı) açık: oyuncu metni zaman yapısı
            // izlenimi veren gün/hafta/ay/yıl/sezon kelimeleri taşımamalı.
            string[] calendarWords = { "gün", "hafta", "ay ", "yıl", "sezon", "bugün", "yarın", "sabah" };
            foreach (string text in AllPlayerVisibleStrings())
            {
                string lowered = text.ToLowerInvariant();
                foreach (string word in calendarWords)
                {
                    Assert.DoesNotContain(word, lowered);
                }
            }
        }

        [Fact]
        public void HerSecimin_KendineOzgunKapanisiVardir()
        {
            string stayClosing = TrainingCampContent.GetClosing(TrainingCampContent.StayChoiceId);
            string roomClosing = TrainingCampContent.GetClosing(TrainingCampContent.RoomChoiceId);

            Assert.False(string.IsNullOrEmpty(stayClosing));
            Assert.False(string.IsNullOrEmpty(roomClosing));
            Assert.NotEqual(roomClosing, stayClosing);
        }

        [Fact]
        public void BilinmeyenSecimKimligi_HataFirlatir()
        {
            Assert.Throws<ArgumentException>(
                () => TrainingCampContent.GetClosing("yok_boyle_bir_secim"));
        }
    }
}
