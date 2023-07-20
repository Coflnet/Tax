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
            // TODO: Get name,lastname and email from payment topic and create a contact here if necessary
            /*
            string? contactId = await taxService.findCustomerContact("test@test.de");
            if (contactId is null)
            {
                contactId = await taxService.createCustomerContact("test", "tester", "test@test.de");
            }
            */

            await taxService.createLexOfficeInvoice(new Voucher()
            {
                VoucherNumber = $"{paymentEvent.ProductId} - {paymentEvent.PaymentMethod}",
                VoucherItems = new List<VoucherItem>(){new VoucherItem(){
                    Amount = (decimal) paymentEvent.PayedAmount,
                    CategoryId = CategoryID.Dienstleistungen,
                    TaxRatePercent = paymentEvent.CountryCode == "DE" ? 19 : 0,

                }},
                UseCollectiveContact = true,
                VoucherDate = paymentEvent.Timestamp,
                Remark = paymentEvent.ProductId
            });
        }, stoppingToken);
    }
}