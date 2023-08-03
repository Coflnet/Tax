using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace Tax.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController : ControllerBase
    {

        private readonly ILogger<TaxController> _logger;
        private readonly TaxService taxService;

        private readonly PayPalService payPalService;
        private readonly StripeService stripeService;

        public TaxController(ILogger<TaxController> logger, TaxService taxService, StripeService stripeService, PayPalService payPalService)
        {
            _logger = logger;
            this.taxService = taxService;
            this.stripeService = stripeService;
            this.payPalService = payPalService;
        }

        [HttpPost]
        [Route("/createTestSalesVoucher")]
        public async void CreateTestSalesVoucher()
        {
            Voucher voucher = new Voucher()
            {
                Type = "salesinvoice",
                VoucherDate = DateTime.Now,
                VoucherNumber = "Vouchernumber-1234",
                VoucherItems = new List<VoucherItem>(){
                new VoucherItem(){
                    Amount = 6.99m,
                    CategoryId = CategoryID.Dienstleistungen,
                    TaxRatePercent = 19
                }
               },
                Remark = "1.800 CoflCoins",
                UseCollectiveContact = true
            };

            try
            {
                await taxService.createLexOfficeInvoice(voucher);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error while posting invoice to lexoffice");
            }
        }

        [HttpPost]
        [Route("/createTestPurchaseVoucher")]
        public async void CreateTestPurchaseVoucher()
        {
            Voucher voucher = new Voucher()
            {
                Type = "purchaseinvoice",
                VoucherDate = DateTime.Now,
                VoucherNumber = "Vouchernumber-1234",
                VoucherItems = new List<VoucherItem>(){
                new VoucherItem(){
                    Amount = 6.99m,
                    CategoryId = CategoryID.Dienstleistungen,
                    TaxRatePercent = 19
                }
               },
                Remark = "1.800 CoflCoins",
                UseCollectiveContact = true
            };

            try
            {
                await taxService.createLexOfficeInvoice(voucher);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error while posting invoice to lexoffice");
            }
        }

        [HttpGet]
        [Route("/getPaypalFeeForTransaction/{id}")]
        public async Task<decimal> GetPaypalFeeForTransaction(string id)
        {
            return await payPalService.getPayPalFeeForTransaction(id);
        }

        [HttpGet]
        [Route("/getStripeFeeForTransaction/{id}")]
        public decimal GetStripeFeeForTransaction(string id)
        {
            return stripeService.getStripeFeeForBalanceTransaction(id);
        }
    }
}
