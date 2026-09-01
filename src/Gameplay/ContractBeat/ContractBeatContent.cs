using System;
using System.Collections.Generic;
using System.Linq;
using Striker.Core.Career;

namespace Striker.Gameplay.ContractBeat
{
    /// <summary>
    /// Contract beat diliminin yazarak (authoring) içeriği. Hepsi özgün IP'dir —
    /// gerçek kulüp, lig veya oyuncu yok. İlerleme sayısal değildir; kritik andaki karar
    /// teklifin "tonunu" şekillendirir ve oyuncu hiçbir sayı görmez.
    /// </summary>
    public static class ContractBeatContent
    {
        public const string TrialsBeatId = "trials";
        public const string FirstBallBeatId = "first_ball";
        public const string OfferBeatId = "offer";
        public const string SignedBeatId = "signed";
        public const string WalkedBeatId = "walked";

        /// <summary>Dilimin beat grafiğini üretir.</summary>
        public static IReadOnlyList<CareerBeat> BuildBeats()
        {
            return new List<CareerBeat>
            {
                new CareerBeat(
                    TrialsBeatId,
                    "Kemalpaşa Dayıları",
                    "Yağmurlu bir sabah, şehrin arka sahalarından birinde seçmelere geldin. "
                    + "Birkaç kulüp antrenörü kenarda seninle konuşmadan önce sana topu verdi.",
                    CareerBeatKind.Narrative,
                    new List<BeatChoice>
                    {
                        new BeatChoice("show_off", "Topuk paslarıyla kendini göstermeye çalış",
                            FirstBallBeatId),
                        new BeatChoice("quiet", "Mütevazı oyna, işine bak", FirstBallBeatId),
                    }),

                new CareerBeat(
                    FirstBallBeatId,
                    "İlk Top",
                    "Son dakika. Top sana geliyor. Antrenörlerin gözü üzerinde.",
                    CareerBeatKind.CriticalMoment,
                    new List<BeatChoice>
                    {
                        new BeatChoice("bold", "Ortayı ara — risk senin olsun", OfferBeatId),
                        new BeatChoice("passive", "Topu güvenliye, sorumluluğu arka hatta bırak",
                            OfferBeatId),
                    }),

                new CareerBeat(
                    OfferBeatId,
                    "Teklif",
                    "Seçme sona erdi. Bir kulüp yöneticisi senden kenara gelmeni istiyor.",
                    CareerBeatKind.Narrative,
                    new List<BeatChoice>
                    {
                        new BeatChoice("accept", "Kalemi uzat, imzala", SignedBeatId),
                        new BeatChoice("decline", "Teşekkür et, bekle", WalkedBeatId),
                    }),

                new CareerBeat(SignedBeatId, "İlk Sözleşme",
                    "Adını kâğıda yazdın. Uzun bir yolun ilk kilometre taşı.",
                    CareerBeatKind.Resolution, new List<BeatChoice>()),

                new CareerBeat(WalkedBeatId, "Bekleyiş",
                    "Mütevazıca reddettin. Kapı senin için hep açık kaldı.",
                    CareerBeatKind.Resolution, new List<BeatChoice>()),
            };
        }

        /// <summary>
        /// Oyuncunun verdiği karar sırasından öneriyi türetir. "Hissedilen, sayılmayan"
        /// ilerlemenin somut taşıyıcısı: kritik andaki cesur karar teklifin tonunu yükseltir.
        /// </summary>
        public static ContractOffer BuildOffer(IReadOnlyList<BeatChoice> ledger)
        {
            bool showedBoldness = ledger.Any(c => c.Id == "bold");

            return showedBoldness
                ? new ContractOffer(
                    "Göl Kenarı Gençlik",
                    "Orta sıra bir gençlik kulübü, maçlarda kendini kanıtlama şansı sunuyor. "
                    + "\"O ancak bizde kendini bulur\" dediler.")
                : new ContractOffer(
                    "Göl Kenarı Gençlik",
                    "Yedeklerde başlayacağın, sabırlı bir gençlik kontratı. "
                    + "\"Zamanla oturur\" dediler.");
        }

        /// <summary>Dilimin akışını tek kompozisyon noktasında kurar (test + editor wiring).</summary>
        public static ContractFlow BuildFlow()
        {
            return new ContractFlow(BuildBeats(), TrialsBeatId, OfferBeatId, BuildOffer);
        }
    }
}