namespace Tax
{
    public class VoucherSearchResponse
    {
        public string Id { get; set; }
        public string VoucherType { get; set; }
        public string VoucherStatus { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime VoucherDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string DueDate { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OpenAmount { get; set; }
        public string Currency { get; set; }
        public bool Archived { get; set; }
    }
}