using Coflnet.Kafka;
using Tax;

public class TaxBackgroundService : BackgroundService
{
    private readonly KafkaConsumer consumer;
    private readonly IConfiguration config;
    private readonly TaxService taxService;
    private readonly StripeService stripeService;
    private readonly PayPalService payPalService;

    public TaxBackgroundService(KafkaConsumer consumer, IConfiguration config, TaxService taxService, StripeService stripeService, PayPalService payPalService)
    {
        this.consumer = consumer;
        this.config = config;
        this.taxService = taxService;
        this.stripeService = stripeService;
        this.payPalService = payPalService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.Consume<PaymentEvent>(config["KAFKA:PAYMENT_TOPIC:NAME"], async (paymentEvent) =>
        {
            decimal fee;
            string contactId;
            switch (paymentEvent.PaymentProvider)
            {
                case "stripe":
                    fee = stripeService.getStripeFeeForBalanceTransaction(paymentEvent.PaymentProviderTransactionId);
                    contactId = ContactId.Stripe;
                    break;
                case "paypal":
                    fee = await payPalService.getPayPalFeeForTransaction(paymentEvent.PaymentProviderTransactionId);
                    contactId = ContactId.PayPal;
                    break;
                default:
                    throw new NotSupportedException($"payment provider {paymentEvent.PaymentProvider} is not supported");
            }

            await taxService.createLexOfficeInvoice(new Voucher()
            {
                Type = "salesinvoice",
                VoucherNumber = $"{paymentEvent.PaymentProviderTransactionId}",
                VoucherItems = new List<VoucherItem>(){new VoucherItem(){
                    Amount = (decimal) paymentEvent.PayedAmount,
                    CategoryId = CategoryID.Dienstleistungen,
                    TaxRatePercent = paymentEvent.CountryCode == "DE" ? 19 : 0,
                }},
                UseCollectiveContact = true,
                VoucherDate = paymentEvent.Timestamp,
                Remark = $"{paymentEvent.PaymentMethod} - {paymentEvent.ProductId}"
            });
            
            await taxService.createLexOfficeInvoice(new Voucher()
            {
                Type = "purchaseinvoice",
                VoucherNumber = $"{paymentEvent.PaymentProviderTransactionId}",
                VoucherItems = new List<VoucherItem>(){new VoucherItem(){
                    Amount = fee,
                    CategoryId = CategoryID.DienstleistungsAusgabe,
                    TaxRatePercent = 0,
                }},
                UseCollectiveContact = false,
                ContactId = contactId,
                VoucherDate = paymentEvent.Timestamp,
                Remark = $"Transaktionsgebühr"
            });
        }, stoppingToken);
    }
}