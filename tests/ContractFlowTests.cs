using System;
using Striker.Core.Career;
using Striker.Gameplay.ContractBeat;
using Xunit;

namespace Striker.Tests
{
    /// <summary>
    /// Contract beat diliminin çekirdek akışını doğrular (engine-bağımsız, sayısal değil).
    /// </summary>
    public class ContractFlowTests
    {
        private ContractFlow CreateFlow() => ContractBeatContent.BuildFlow();

        [Fact]
        public void Akis_TrialsIleBaslar_VeOyunculuHicBirSayiGormez()
        {
            ContractFlow flow = CreateFlow();

            Assert.Equal(ContractBeatContent.TrialsBeatId, flow.CurrentBeat.Id);
            Assert.Equal(CareerBeatKind.Narrative, flow.CurrentBeat.Kind);
            Assert.Empty(flow.ChoiceLedger);
            Assert.False(flow.IsResolved);
            Assert.Null(flow.Offer);
        }

        [Fact]
        public void Karar_Verince_Oyun_SonrakiBeateIlerler()
        {
            ContractFlow flow = CreateFlow();

            flow.Choose("show_off");

            Assert.Single(flow.ChoiceLedger);
            Assert.Equal(ContractBeatContent.FirstBallBeatId, flow.CurrentBeat.Id);
            Assert.Equal(CareerBeatKind.CriticalMoment, flow.CurrentBeat.Kind);
        }

        [Fact]
        public void TeklifBeatiDahilEdilince_KontratOnerisiUretilir()
        {
            ContractFlow flow = CreateFlow();

            flow.Choose("quiet");
            flow.Choose("passive");

            Assert.Equal(ContractBeatContent.OfferBeatId, flow.CurrentBeat.Id);
            Assert.NotNull(flow.Offer);
            Assert.Equal("Göl Kenarı Gençlik", flow.Offer!.ClubName);
            Assert.False(flow.IsResolved);
        }

        [Fact]
        public void KritikAnCesurKarari_TeklifTonunuDegistirir_VeSayiKullanmaz()
        {
            ContractFlow bold = CreateFlow();
            bold.Choose("show_off");
            bold.Choose("bold");
            // cesur → teklif, "kanıtlama şansı" vurgusu taşır

            ContractFlow passive = CreateFlow();
            passive.Choose("quiet");
            passive.Choose("passive");
            // pasif → teklif, "sabırlı başlangıç" vurgusu taşır

            Assert.NotEqual(0, string.CompareOrdinal(bold.Offer!.Terms, passive.Offer!.Terms));
            // Hiçbir beat sayısal bir değer taşımıyor (felsefe: felt, not counted).
            Assert.Contains("kanıtlama", bold.Offer!.Terms);
            Assert.Contains("sabırlı", passive.Offer!.Terms);
        }

        [Fact]
        public void TeklifiKabulEtme_AkisiKontratliSonucaBaglar()
        {
            ContractFlow flow = CreateFlow();
            flow.Choose("quiet");
            flow.Choose("bold");

            flow.Decide(accepted: true);

            Assert.True(flow.IsResolved);
            Assert.Equal(ContractBeatContent.SignedBeatId, flow.CurrentBeat.Id);
            Assert.Equal(CareerBeatKind.Resolution, flow.CurrentBeat.Kind);
            Assert.Equal(3, flow.ChoiceLedger.Count);
        }

        [Fact]
        public void TeklifiReddetme_AkisiBekleyisSonucunaBaglar()
        {
            ContractFlow flow = CreateFlow();
            flow.Choose("quiet");
            flow.Choose("passive");

            flow.Decide(accepted: false);

            Assert.True(flow.IsResolved);
            Assert.Equal(ContractBeatContent.WalkedBeatId, flow.CurrentBeat.Id);
            Assert.Equal(CareerBeatKind.Resolution, flow.CurrentBeat.Kind);
        }

        [Fact]
        public void BilinmeyenKarar_VergiFırlatır()
        {
            ContractFlow flow = CreateFlow();

            Assert.Throws<ArgumentException>(() => flow.Choose("non-existent"));
        }

        [Fact]
        public void TeklifUretilmedenKarar_GeçerliDegildir()
        {
            ContractFlow flow = CreateFlow();

            Assert.Throws<InvalidOperationException>(() => flow.Decide(accepted: true));
        }

        [Fact]
        public void SonuclanmisAkışaKarar_Gecersizdir()
        {
            ContractFlow flow = CreateFlow();
            flow.Choose("quiet");
            flow.Choose("passive");
            flow.Decide(accepted: true);

            Assert.Throws<InvalidOperationException>(() => flow.Choose("accept"));
        }
    }
}
