namespace Tax
{
    public class VoucherItem
    {
        public required decimal Amount { get; set; }
        public decimal TaxAmount
        {
            get
            {
                return Math.Round((Amount / (100 + TaxRatePercent)) * TaxRatePercent, 2);
            }
        }
        public required decimal TaxRatePercent { get; set; }
        public required string CategoryId { get; set; }
    }
}