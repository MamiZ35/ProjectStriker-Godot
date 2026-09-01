using System;

namespace Striker.Core.Career
{
    /// <summary>
    /// Kalıcı kariyer kaydı: oyuncunun yarattığı kimlik (ad + rol) ve bu kariyerin
    /// sözleşmeyle imzalanıp imzalanmadığı. Motor-bağımsızdır (engine-free):
    /// dosya erişimi ayrı katmandadır; bu sınıf yalnızca veriyi ve basit seri
    /// hale getirmeyi taşır, böylece mantık testleri Godot'suz çalışır.
    /// </summary>
    public sealed class CareerSave
    {
        /// <summary>Kalıcı kimlik (ad + rol).</summary>
        public PlayerIdentity Identity { get; }

        /// <summary>
        /// Kariyer sözleşmeyle İMZALANDI mı? TrainingCamp'e tek giriş bu işaret
        /// üzerinden korunur (tasarım kuralı: TrainingCamp'e yalnızca signed
        /// sondan girilir; CONTINUE doğrudan bu kayda dayanır).
        /// </summary>
        public bool Signed { get; }

        public CareerSave(PlayerIdentity identity, bool signed)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            Signed = signed;
        }

        // ------------------------------------------------------------------
        // Seri hale getirme (basit, satır bazlı, sürüm işaretli)
        // ------------------------------------------------------------------

        /// <summary>Servis katmanı bu satırı disk'e (user://) yazar.</summary>
        public const string FormatVersion = "striker.save.v1";

        public string ToSaveString()
        {
            var lines = new[]
            {
                FormatVersion,
                "name=" + Identity.Name,
                "role=" + Identity.Role.ToString(),
                "signed=" + (Signed ? "1" : "0"),
            };

            return string.Join("\n", lines) + "\n";
        }

        /// <summary>
        /// Dizi içeriden CareerSave'ı geri oluşturur. Geçersiz ya da sürümü
        /// tanınmayan içerik için null döner (savunmacı; bozuk kayıt oyunu
        /// çökertmez).
        /// </summary>
        public static CareerSave? FromSaveString(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            string[] lines = data.Replace("\r\n", "\n").Trim().Split('\n');
            if (lines.Length < 2 || lines[0] != FormatVersion)
            {
                return null;
            }

            string? name = null;
            PlayerRole? role = null;
            bool signed = false;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                int sep = line.IndexOf('=');
                if (sep <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, sep).Trim();
                string value = line.Substring(sep + 1).Trim();

                switch (key)
                {
                    case "name":
                        name = value;
                        break;
                    case "role":
                        if (Enum.TryParse<PlayerRole>(value, out PlayerRole parsed))
                        {
                            role = parsed;
                        }
                        break;
                    case "signed":
                        signed = value == "1";
                        break;
                }
            }

            if (name == null || role == null)
            {
                return null;
            }

            return new CareerSave(new PlayerIdentity(name, role.Value), signed);
        }
    }
}
