namespace Pausalio.Application.DTOs.KpoBook
{
    public class KpoAlertDto
    {
        public string ThresholdName { get; set; } = null!;
        public decimal ThresholdAmount { get; set; }
        public decimal CurrentTotal { get; set; }
        public bool IsExceeded { get; set; }
    }
}
