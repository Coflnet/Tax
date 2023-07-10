using Coflnet.Kafka;
using Tax;

public class TaxBackgroundService : BackgroundService
{
    private readonly KafkaConsumer consumer;
    private readonly IConfiguration config;
    private readonly TaxService taxService;

    public TaxBackgroundService(KafkaConsumer consumer, IConfiguration config, TaxService taxService)
    {
        this.consumer = consumer;
        this.config = config;
        this.taxService = taxService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.Consume<PaymentEvent>(config["KAFKA:PAYMENT_TOPIC:NAME"], async (paymentEvent) =>
        {
            await taxService.createLexOfficeInvoice(new Invoice()
            {
                Address = new Address()
                {
                    Name = paymentEvent.UserId,
                    Zip = paymentEvent.PostalCode,
                    CountryCode = paymentEvent.CountryCode,
                },
                Items = new InvoiceItem[]{new CustomInvoiceItem(){
                    Name = $"{paymentEvent.ProductId} - {paymentEvent.PaymentMethod}",
                    Quantity = 1,
                    UnitPrice = new UnitPrice(){
                        Currency = paymentEvent.Currency,
                        GrossAmount = (decimal) paymentEvent.PayedAmount,
                        TaxRatePercentage = paymentEvent.CountryCode.Equals("DE") ? 19 : 0
                    },

                }, new TextInvoiceItem(){
                    Name = "Transaction-ID",
                    Description = paymentEvent.PaymentProviderTransactionId
                }},
                VoucherDate = paymentEvent.Timestamp
            });
        }, stoppingToken);
    }
}