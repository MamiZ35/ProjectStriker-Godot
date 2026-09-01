using System.Collections.Generic;

namespace Striker.Core.Career
{
    /// <summary>
    /// Kariyerin tek bir anı: oyuncuya sunulan durum ve ondan sonra gelen karar(lar).
    /// Sayısal mekanik yoktur — "oyuncu bu anı nasıl yaşar" belirleyicidir.
    /// </summary>
    public sealed class CareerBeat
    {
        /// <summary>Rota içinde beat'i benzersizleştiren tanımlayıcı.</summary>
        public string Id { get; }

        /// <summary>Oyuncuya gösterilecek başlık.</summary>
        public string Title { get; }

        /// <summary>Beat'in anlatı metni.</summary>
        public string Body { get; }

        /// <summary>Beat türü (anlatı / kritik an / sonuç).</summary>
        public CareerBeatKind Kind { get; }

        /// <summary>Oyuncuya sunulan kararlar. Sonuç beat'lerinde boş olabilir.</summary>
        public IReadOnlyList<BeatChoice> Choices { get; }

        public CareerBeat(
            string id,
            string title,
            string body,
            CareerBeatKind kind,
            IReadOnlyList<BeatChoice> choices)
        {
            Id = id;
            Title = title;
            Body = body;
            Kind = kind;
            Choices = choices;
        }
    }
}