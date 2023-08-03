using Stripe;

public class StripeService
{
    public IConfiguration config;

    public StripeService(IConfiguration config)
    {
        this.config = config;
        StripeConfiguration.ApiKey = config["STRIPE:KEY"];
    }

    public decimal getStripeFeeForBalanceTransaction(string paymentIntentId)
    {
        PaymentIntentService paymentIntentService = new PaymentIntentService();
        ChargeService chargeService = new ChargeService();
        BalanceTransactionService balanceTransactionService = new BalanceTransactionService();

        PaymentIntent paymentIntent = paymentIntentService.Get(paymentIntentId);
        Charge charge = chargeService.Get(paymentIntent.LatestChargeId);
        BalanceTransaction transaction = balanceTransactionService.Get(charge.BalanceTransactionId);
        decimal fee = Convert.ToDecimal(transaction.Fee) / 100m;
        return fee;
    }
}
