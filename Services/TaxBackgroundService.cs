using Coflnet.Kafka;
using Tax;

public class TaxBackgroundService : BackgroundService
{
    private readonly KafkaConsumer consumer;
    private readonly IConfiguration config;
    private readonly TaxService taxService;
    private readonly StripeService stripeService;
    private readonly PayPalService payPalService;
    private readonly ILogger<TaxBackgroundService> logger;

    public TaxBackgroundService(KafkaConsumer consumer, IConfiguration config, TaxService taxService, StripeService stripeService, PayPalService payPalService, ILogger<TaxBackgroundService> logger)
    {
        this.consumer = consumer;
        this.config = config;
        this.taxService = taxService;
        this.stripeService = stripeService;
        this.payPalService = payPalService;
        this.logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.Consume<PaymentEvent>(config["KAFKA:PAYMENT_TOPIC:NAME"], async (paymentEvent) =>
        {
            decimal fee;
            string contactId;
            await Task.Delay(TimeSpan.FromMinutes(1));
            logger.LogInformation($"Creating an Invoice for {paymentEvent.PaymentProviderTransactionId}");
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
                    TaxRatePercent = paymentEvent.Address.CountryCode == "DE" ? 19 : 0,
                }},
                UseCollectiveContact = true,
                VoucherDate = paymentEvent.Timestamp,
                Remark = $"{paymentEvent.PaymentMethod} - {paymentEvent.ProductId}"
            });
            logger.LogInformation("Created an Invoice");

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
            logger.LogInformation("Created a purchase Invoice for fees");
            await Task.Delay(TimeSpan.FromMinutes(10));
        }, stoppingToken, config["KAFKA:PAYMENT_TOPIC:GROUP"] ?? throw new ArgumentNullException("KAFKA:PAYMENT_TOPIC:GROUP"));
    }
}