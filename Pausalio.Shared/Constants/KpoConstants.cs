namespace Pausalio.Shared.Constants
{
    /// <summary>
    /// Konstante za KPO (Knjiga o ostvarenom prometu paušalno oporezovanih obveznika).
    /// </summary>
    public static class KpoConstants
    {
        /// <summary>
        /// Osnova knjiženja u KPO:
        ///   "InvoiceDate" = datum prometa / izdavanja fakture (IssueDate).
        ///   Alternativa: "PaymentDate" = datum uplate na račun.
        /// Potvrdi sa računovođom pre promene ovog parametra.
        /// </summary>
        public const string RecordingBasis = "InvoiceDate";

        /// <summary>Upozorenje na prihod od 4.000.000 RSD.</summary>
        public const decimal Threshold4M = 4_000_000m;

        /// <summary>Upozorenje na prihod od 5.000.000 RSD.</summary>
        public const decimal Threshold5M = 5_000_000m;

        /// <summary>Godišnji limit za paušalne preduzetnike — 6.000.000 RSD.</summary>
        public const decimal LimitSelfEmployed = 6_000_000m;

        /// <summary>Prag za obaveznu PDV registraciju — 8.000.000 RSD.</summary>
        public const decimal LimitVatRegistration = 8_000_000m;
    }
}
