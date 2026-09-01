using System;
using System.Collections.Generic;
using System.Linq;

namespace Striker.Core.Career
{
    /// <summary>
    /// Contract beat diliminin çekirdek akışı: trials → kritik an → kontrat teklifi → kabul/red.
    /// Saf (engine-bağımsız) ve sayısızdır: ilerleme yalnızca oyuncunun aldığı kararların
    /// sırası (ledger) üzerinden hissedilir, oyuncuya hiçbir sayı gösterilmez.
    /// </summary>
    public sealed class ContractFlow
    {
        private readonly Dictionary<string, CareerBeat> _beats;
        private readonly string _offerBeatId;
        private readonly Func<IReadOnlyList<BeatChoice>, ContractOffer> _offerFactory;

        private readonly List<BeatChoice> _choiceLedger = new();
        private ContractOffer? _offer;

        /// <summary>Oyunun şu an bulunduğu beat.</summary>
        public CareerBeat CurrentBeat => _beats[_currentBeatId];

        /// <summary>Kontrat teklifi oluştuysa erişilebilir; henüz yoksa null.</summary>
        public ContractOffer? Offer => _offer;

        /// <summary>Oyuncunun akış boyunca verdiği kararların sırası.</summary>
        public IReadOnlyList<BeatChoice> ChoiceLedger => _choiceLedger;

        /// <summary>Akil kabul/red ile sonuçlandı mı?</summary>
        public bool IsResolved { get; private set; }

        private string _currentBeatId;

        public ContractFlow(
            IEnumerable<CareerBeat> beats,
            string startBeatId,
            string offerBeatId,
            Func<IReadOnlyList<BeatChoice>, ContractOffer> offerFactory)
        {
            _beats = beats.ToDictionary(b => b.Id);
            _currentBeatId = startBeatId;
            _offerBeatId = offerBeatId;
            _offerFactory = offerFactory;

            if (!_beats.ContainsKey(startBeatId))
            {
                throw new ArgumentException($"Başlangıç beat'i rota içinde yok: {startBeatId}", nameof(startBeatId));
            }
        }

        /// <summary>
        /// Oyuncu şu anaı bir karar verir ve rota bir sonraki beat'e ilerler.
        /// Hedef teklif beat'i ise o noktada kontrat teklifi üretilir.
        /// </summary>
        public void Choose(string choiceId)
        {
            if (IsResolved)
            {
                throw new InvalidOperationException("Akış çoktan sonuçlandı; yeni karar alınamaz.");
            }

            BeatChoice? match = CurrentBeat.Choices.FirstOrDefault(c => c.Id == choiceId);
            if (match is null)
            {
                throw new ArgumentException($"'{_currentBeatId}' beat'i '{choiceId}' kararını içermiyor.", nameof(choiceId));
            }

            _currentBeatId = match.NextBeatId;
            _choiceLedger.Add(match);

            if (_currentBeatId == _offerBeatId)
            {
                _offer = _offerFactory(_choiceLedger);
            }
        }

        /// <summary>
        /// Teklif verildikten sonra oyuncu kabul/red kararı verir ve akışı sonuçlandırır.
        /// Kabul ve red beat'leri rotanın kalıcı beat'leri olarak normal şekilde ilerler;
        /// ayrım yalnızca oyuncunun seçtiği kararda gizlidir.
        /// </summary>
        public void Decide(bool accepted)
        {
            if (IsResolved)
            {
                throw new InvalidOperationException("Akış zaten sonuçlandı.");
            }

            if (_offer is null)
            {
                throw new InvalidOperationException("Teklif üretilmeden karar verilemez; önce teklif beat'ine ulaşın.");
            }

            // Kabul/red, mevcut (teklif) beat'inin "accept"/"decline" kararları üzerinden ilerler.
            string decisionId = accepted ? "accept" : "decline";
            BeatChoice? decision = CurrentBeat.Choices.FirstOrDefault(c => c.Id == decisionId);
            if (decision is null)
            {
                throw new InvalidOperationException("Teklif beat'i kabul/red kararlarını içermiyor.");
            }

            _currentBeatId = decision.NextBeatId;
            _choiceLedger.Add(decision);
            IsResolved = true;
        }
    }
}