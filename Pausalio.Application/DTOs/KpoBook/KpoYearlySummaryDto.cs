namespace Pausalio.Application.DTOs.KpoBook
{
    public class KpoYearlySummaryDto
    {
        public int Year { get; set; }
        public decimal TotalProductIncome { get; set; }
        public decimal TotalServiceIncome { get; set; }
        public decimal GrandTotal { get; set; }
        public int EntryCount { get; set; }
        public decimal RemainingTo6MLimit { get; set; }
        public decimal RemainingTo8MLimit { get; set; }

        /// <summary>
        /// PDV izloženost — ukupan promet BEZ usluga čije je mesto prometa u inostranstvu.
        /// Koristi se za praćenje 8M PDV praga.
        /// </summary>
        public decimal VatExposureTotal { get; set; }

        public List<KpoAlertDto> Alerts { get; set; } = new();
    }
}
