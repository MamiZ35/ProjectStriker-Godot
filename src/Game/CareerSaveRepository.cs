using Godot;
using Striker.Core.Career;

namespace Striker.Game
{
    /// <summary>
    /// Kalıcı kariyer kaydının stateless deposu (user://career.save). Motor-bağımsız
    /// veri ve seri hale getirme <see cref="CareerSave"/> içindedir; bu sınıf yalnızca
    /// Godot FileAccess üzerinden dosyayı okur/yazar. Her çağrı disk'e gider, bu
    /// yüzden headless flow test'lerinde (autoload yok) da güvenle çalışır.
    ///
    /// Akış (Faz 7):
    ///   PlayerCreation  → BeginCareer(name, role)   (signed=false, kimlik kalıcı)
    ///   ContractBeat    → CommitCareer()            (signed=true, yalnızca kimlik varsa)
    ///   MainMenu        → HasSave()/Load()          (CONTINUE aktif + yönlendirme)
    /// </summary>
    public static class CareerSaveRepository
    {
        private const string SavePath = "user://career.save";

        /// <summary>Kayıt dosyası var mı?</summary>
        public static bool HasSave() => FileAccess.FileExists(SavePath);

        /// <summary>
        /// Disk'teki kaydı okur. Yoksa/okunamıyorsa/bozuksa null döner —
        /// bozuk kayıt oyunu çökertmez (savunmacı).
        /// </summary>
        public static CareerSave? Load()
        {
            if (!FileAccess.FileExists(SavePath))
            {
                return null;
            }

            using (var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read))
            {
                return file != null ? CareerSave.FromSaveString(file.GetAsText()) : null;
            }
        }

        /// <summary>Kimliği diske yazar (signed=false ile yeni kariyer).</summary>
        public static void BeginCareer(string name, PlayerRole role)
        {
            Save(new CareerSave(new PlayerIdentity(name, role), signed: false));
        }

        /// <summary>
        /// Kariyer imzalandı: mevcut kaydı signed=true ile yazar. Kimlik yoksa no-op.
        /// </summary>
        public static void CommitCareer()
        {
            CareerSave? current = Load();
            if (current == null)
            {
                return;
            }

            Save(new CareerSave(current.Identity, signed: true));
        }

        /// <summary>Kaydı siler (temizlik/test).</summary>
        public static void Clear()
        {
            if (FileAccess.FileExists(SavePath))
            {
                DirAccess.RemoveAbsolute(SavePath);
            }
        }

        private static void Save(CareerSave save)
        {
            using (var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write))
            {
                file?.StoreString(save.ToSaveString());
            }
        }
    }
}
