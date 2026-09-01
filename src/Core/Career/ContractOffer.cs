namespace Striker.Core.Career
{
    /// <summary>
    /// Trials sonrası teklif edilen kontrat. Özgün IP olduğu için gerçek kulüp yok;
    /// teklif, sayısal değil nitel kavramlarla ifade edilir ("mütevazı ikinci lig kulübü").
    /// </summary>
    public sealed class ContractOffer
    {
        /// <summary>Teklif eden kulübün özgün, kurgusal adı.</summary>
        public string ClubName { get; }

        /// <summary>Teklifin nitel anlatımı (oyuncunun gördüğü metin).</summary>
        public string Terms { get; }

        public ContractOffer(string clubName, string terms)
        {
            ClubName = clubName;
            Terms = terms;
        }
    }
}