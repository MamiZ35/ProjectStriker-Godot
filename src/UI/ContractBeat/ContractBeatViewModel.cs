using System;
using System.Collections.Generic;
using Striker.Core.Career;
using Striker.Gameplay.ContractBeat;

namespace Striker.UI.ContractBeat
{
    /// <summary>
    /// Contract beat dilimini ekrana sunmak için engine-bağımsız sunum modeli.
    /// "Hissedilen, sayılmayan" ilerlemeyi korur: oyuncuya hiçbir sayı asla
    /// gösterilmez — tüm görünen metinler niteldir ve test bu garantiyi doğrular.
    /// </summary>
    public sealed class ContractBeatViewModel
    {
        private readonly ContractFlow _flow;

        public ContractBeatViewModel(ContractFlow flow)
        {
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
            Refresh();
        }

        /// <summary>Şu anki beat'in oyuncuya gösterilen başlığı.</summary>
        public string Title { get; private set; } = string.Empty;

        /// <summary>Beat anlatısı; teklif beat'inde türetilen teklif metni eklenir.</summary>
        public string BodyText { get; private set; } = string.Empty;

        /// <summary>Oyuncunun seçebileceği kararlar (sonuç beat'lerinde boş).</summary>
        public IReadOnlyList<BeatChoice> Options { get; private set; } = Array.Empty<BeatChoice>();

        /// <summary>Akış kabul/red ile sonuçlandı mı?</summary>
        public bool IsResolved => _flow.IsResolved;

        /// <summary>
        /// Akış İMZALANMIŞ sözleşmeyle sonuçlandı mı? (Sprint 10: yalnızca bu
        /// durumda ekran "İLK ANTRENMAN" geçiş affordansını gösterir; Bekleyiş
        /// yolu olduğu gibi kalır.) Başlık metnine string eşleme YOK — tipli sinyal.
        /// </summary>
        public bool IsSignedResolution => _flow.IsResolved && _flow.CurrentBeat.Id == ContractBeatContent.SignedBeatId;

        /// <summary>Oyun şu an teklif beat'inde mi? (kabul/red bu beat'te kararlaştırılır)</summary>
        public bool IsOnOfferBeat => _flow.CurrentBeat.Id == ContractBeatContent.OfferBeatId;

        /// <summary>Kararı akışa iletir (teklif beat'i dışındaki adımlar için).</summary>
        public void Choose(string choiceId)
        {
            _flow.Choose(choiceId);
            Refresh();
        }

        /// <summary>Teklif beat'inde kabul/red kararını akışa iletir.</summary>
        public void Decide(bool accepted)
        {
            _flow.Decide(accepted);
            Refresh();
        }

        private void Refresh()
        {
            CareerBeat beat = _flow.CurrentBeat;
            Title = beat.Title;
            BodyText = beat.Body;

            if (beat.Id == ContractBeatContent.OfferBeatId && _flow.Offer is ContractOffer offer)
            {
                BodyText = BodyText + "\n\n" + offer.ClubName + "\n" + offer.Terms;
            }

            Options = beat.Choices;
        }
    }
}
