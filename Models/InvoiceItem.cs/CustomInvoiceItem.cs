namespace Tax
{
    public class CustomInvoiceItem : InvoiceItem
    {
        public override string Type => "custom";
        public required int Quantity { get; set; }
        public string? UnitName { get; set; } = "Stück";
        public required UnitPrice UnitPrice { get; set; }
    }
}