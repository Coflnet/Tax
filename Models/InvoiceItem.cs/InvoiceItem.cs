
public abstract class InvoiceItem
{
    public virtual string Type { get; } = "";
    public required string Name { get; set; }
}