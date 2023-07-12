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

        public TaxController(ILogger<TaxController> logger, TaxService taxService)
        {
            _logger = logger;
            this.taxService = taxService;
        }

        [HttpPost]
        [Route("/createTestInvoice")]
        public async void Post()
        {
            Invoice invoice = new Invoice()
            {
                Address = new Address()
                {
                    Name = "User-1234",
                    Zip = "00000",
                    CountryCode = "DE"
                },
                Items = new InvoiceItem[]{new CustomInvoiceItem(){
                    Name = "1.800 CoflCoins - PayPal",
                    Quantity = 1,
                    UnitPrice = new UnitPrice(){
                        Currency = "EUR",
                        GrossAmount = 6.99m,
                        TaxRatePercentage = 19
                    }
                }, new TextInvoiceItem(){
                    Name = "Transaction-ID",
                    Description = "Transaction-12345"
                }},
                Currency = "EUR",
                VoucherDate = DateTime.Now
            };

            try
            {
                await taxService.createLexOfficeInvoice(invoice);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.GetBaseException(), "Error while posting invoice to lexoffice");
            }
        }

        [HttpGet]
        [Route("/getLastInvoices")]
        public async Task<IEnumerable<VoucherSearchResponse>> Get()
        {
            return await taxService.getInvoicesForVoucherDay(DateTime.Now);
        }
    }
}
