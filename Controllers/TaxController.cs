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
        [Route("/createTestVoucher")]
        public async void Post()
        {
            Voucher voucher = new Voucher()
            {
                VoucherDate = DateTime.Now,
                VoucherNumber = "Vouchernumber-1234",
                VoucherItems = new List<VoucherItem>(){
                new VoucherItem(){
                    Amount = 6.99m,
                    CategoryId = CategoryID.Dienstleistungen,
                    TaxRatePercent = 19
                }
               },
                Remark = "1.800 CoflCoins"
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
    }
}
