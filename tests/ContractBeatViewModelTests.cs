using Striker.Gameplay;
using Striker.Gameplay.ContractBeat;
using Striker.UI.ContractBeat;
using Xunit;

namespace Striker.Tests
{
    /// <summary>
    /// Contract beat sunum modelini doğrular: beat akışı, türetilen teklif tonu,
    /// sonuçlanma ve en önemlisi — oyuncuya hiçbir sayı gösterilmemesi garantisi.
    /// </summary>
    public class ContractBeatViewModelTests
    {
        private static ContractBeatViewModel CreateViewModel() =>
            new ContractBeatViewModel(ContractBeatContent.BuildFlow());

        private static void AssertNoNumbersVisible(ContractBeatViewModel viewModel)
        {
            AssertNoNumbersVisible(viewModel.Title, viewModel.BodyText);

            foreach (var option in viewModel.Options)
            {
                AssertNoNumbersVisible(option.Label);
            }
        }

        private static void AssertNoNumbersVisible(params string[] texts)
        {
            foreach (string text in texts)
            {
                Assert.False(
                    System.Text.RegularExpressions.Regex.IsMatch(text, "[0-9]"),
                    "Oyuncunun göreceği metinde sayı olmamalı: \"" + text + "\"");
            }
        }

        [Fact]
        public void Baslangic_TrialsBeatiniSunar_VeHicSayiGostermez()
        {
            ContractBeatViewModel viewModel = CreateViewModel();

            Assert.Equal("Kemalpaşa Dayıları", viewModel.Title);
            Assert.Equal(2, viewModel.Options.Count);
            Assert.False(viewModel.IsResolved);
            Assert.False(viewModel.IsOnOfferBeat);
            AssertNoNumbersVisible(viewModel);
        }

        [Fact]
        public void KararVerince_SonrakiBeatSunulur_VeHicSayiGostermez()
        {
            ContractBeatViewModel viewModel = CreateViewModel();

            viewModel.Choose("show_off");

            Assert.Equal("İlk Top", viewModel.Title);
            Assert.Equal(2, viewModel.Options.Count);
            Assert.False(viewModel.IsOnOfferBeat);
            AssertNoNumbersVisible(viewModel);
        }

        [Fact]
        public void TeklifBeatinde_TuretilenTeklifGorunur()
        {
            ContractBeatViewModel viewModel = CreateViewModel();

            viewModel.Choose("quiet");
            viewModel.Choose("passive");

            Assert.True(viewModel.IsOnOfferBeat);
            Assert.Contains("Göl Kenarı Gençlik", viewModel.BodyText);
            Assert.Equal(2, viewModel.Options.Count);
            AssertNoNumbersVisible(viewModel);
        }

        [Fact]
        public void KritikAnCesurKarari_TeklifTonunuDegistirir_VeSayiKullanmaz()
        {
            ContractBeatViewModel bold = CreateViewModel();
            bold.Choose("show_off");
            bold.Choose("bold");

            ContractBeatViewModel passive = CreateViewModel();
            passive.Choose("quiet");
            passive.Choose("passive");

            Assert.Contains("kendini kanıtlama", bold.BodyText);
            Assert.Contains("sabırlı", passive.BodyText);
            Assert.NotEqual(passive.BodyText, bold.BodyText);
            AssertNoNumbersVisible(bold);
            AssertNoNumbersVisible(passive);
        }

        [Fact]
        public void KabulKarari_AkisiSonuclandirir_VeImzaBeatiniGosterir()
        {
            ContractBeatViewModel viewModel = CreateViewModel();
            viewModel.Choose("show_off");
            viewModel.Choose("bold");

            viewModel.Decide(true);

            Assert.True(viewModel.IsResolved);
            Assert.Equal("İlk Sözleşme", viewModel.Title);
            Assert.Empty(viewModel.Options);
            AssertNoNumbersVisible(viewModel);
        }

        [Fact]
        public void RedKarari_AkisiSonuclandirir_VeBekleyisBeatiniGosterir()
        {
            ContractBeatViewModel viewModel = CreateViewModel();
            viewModel.Choose("quiet");
            viewModel.Choose("passive");

            viewModel.Decide(false);

            Assert.True(viewModel.IsResolved);
            Assert.Equal("Bekleyiş", viewModel.Title);
            Assert.Empty(viewModel.Options);
            AssertNoNumbersVisible(viewModel);
        }

        [Fact]
        public void ImzaliCozum_TipliSinyalTasir_BekleyisTasimaz()
        {
            // Sprint 10: ekran yalnızca İMZALANMIŞ sondan antrenmana geçer;
            // sinyal başlık metnine string eşlemeyle DEĞİL, tipli bayrakla taşınır.
            ContractBeatViewModel signed = CreateViewModel();
            signed.Choose("show_off");
            signed.Choose("bold");
            signed.Decide(true);

            ContractBeatViewModel walked = CreateViewModel();
            walked.Choose("quiet");
            walked.Choose("passive");
            walked.Decide(false);

            Assert.True(signed.IsSignedResolution, "İmzalı son IsSignedResolution bayrağını taşımalı.");
            Assert.False(walked.IsSignedResolution, "Bekleyiş yolu antrenman geçişi açMAMALI.");
        }
    }
}
