namespace Tax
{
    public class TextInvoiceItem : InvoiceItem
    {
        public override string Type => "text";
        public required string Description { get; set; }
    }
}