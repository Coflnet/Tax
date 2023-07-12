namespace Tax
{
    public class Invoice
    {
        public required Address Address { get; set; }
        public required InvoiceItem[] Items { get; set; }
        public required DateTime VoucherDate { get; set; }
        public string? Currency { get; set; }

    }
}