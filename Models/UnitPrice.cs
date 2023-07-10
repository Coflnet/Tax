namespace Tax
{
    public class UnitPrice
    {
        public required decimal GrossAmount;
        public double TaxRatePercentage = 0;
        public required string Currency;

    }
}