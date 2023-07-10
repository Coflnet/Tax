namespace Tax
{
    public class Invoice
    {
        public required Address Address { get; set; }
        public required InvoiceItem[] Items { get; set; }
        public string? Title { get; set; }
        public string? Introduction { get; set; }
        public required DateTime VoucherDate { get; set; }
        public string? Remark { get; set; }
        public string? Currency { get; set; }

    }
}