namespace Striker.Core.Career
{
    /// <summary>
    /// Oyuncunun yarattığı futbolcu kimliği: isim + kimlik rolü. Motor-bağımsızdır
    /// (engine-free), basit değer semantiği taşır; bu sprintte hiçbir sistem bu
    /// veriyi tüketmez — sonuçlar v1'de yalnızca anlatı düzeyindedir (DH-001/DEC-017).
    /// </summary>
    public sealed class PlayerIdentity
    {
        /// <summary>Futbolcunun adı; özgün IP Türkçe öneri havuzundan üretilir ve serbestçe düzenlenebilir.</summary>
        public string Name { get; }

        /// <summary>Seçilen kimlik rolü (Golcü / Oyun Kurucu / Kanat / Defans).</summary>
        public PlayerRole Role { get; }

        public PlayerIdentity(string name, PlayerRole role)
        {
            Name = name;
            Role = role;
        }
    }
}
