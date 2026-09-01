namespace Striker.Core.Career
{
    /// <summary>
    /// Bir kariyer beat'inin türü. Oyun "hissedilen, sayılmayan" ilerlemeyi hedefler;
    /// bu yüzden beat'ler sayısal ödül değil, oyuncuya sunulan anlar ve kararlardır.
    /// </summary>
    public enum CareerBeatKind
    {
        /// <summary>Olay akışını taşıyan anlatı anaı.</summary>
        Narrative,

        /// <summary>Kariyerin dönüm noktası; oyuncunun kararı sonucu hissettirir.</summary>
        CriticalMoment,

        /// <summary>Oyunun sonucunu (kontrat kabul/red) sabitleyen an.</summary>
        Resolution,
    }
}