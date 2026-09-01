namespace Striker.Core.Career
{
    /// <summary>
    /// Oyuncunun bir beat'te alabileceği tek karar.
    /// İlerleme "hissedilen, sayılmayan" olduğu için kararın tek sonucu
    /// gittiği hedef beat'tir; sayısal değer taşımaz.
    /// </summary>
    public sealed class BeatChoice
    {
        /// <summary>Beat içinde kararı benzersizleştiren tanımlayıcı.</summary>
        public string Id { get; }

        /// <summary>Oyuncunun gördüğü seçenek etiketi.</summary>
        public string Label { get; }

        /// <summary>Bu karar seçilirse gidilecek hedef beat'in tanımlayıcısı.</summary>
        public string NextBeatId { get; }

        public BeatChoice(string id, string label, string nextBeatId)
        {
            Id = id;
            Label = label;
            NextBeatId = nextBeatId;
        }
    }
}