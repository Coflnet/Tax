using System.Collections;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Tax;

public class TaxService
{
    public IConfiguration config;

    public TaxService(IConfiguration config)
    {
        this.config = config;
    }

    public async Task createLexOfficeInvoice(Voucher voucher)
    {
        bool doesInvoiceAlreadyExist = await doesInvoiceAlreadyExists(voucher);
        if (doesInvoiceAlreadyExist)
        {
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                type = voucher.Type,
                voucherNumber = voucher.VoucherNumber,
                voucherDate = voucher.VoucherDate.ToString("yyyy-MM-dd"),
                totalGrossAmount = voucher.TotalGrossAmount,
                totalTaxAmount = voucher.TotalTaxAmount,
                taxType = voucher.TaxType,
                voucherItems = voucher.VoucherItems,
                useCollectiveContact = voucher.UseCollectiveContact,
                remark = "1.800 CoflCoins"
            };
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, serializerSettings);
            Console.WriteLine(jsonBody);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.PostAsync("https://api.lexoffice.io/v1/vouchers", content);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<bool> doesInvoiceAlreadyExists(Voucher newInvoice)
    {
        using (HttpClient client = new HttpClient())
        {
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            HttpResponseMessage response = await client.GetAsync($"https://api.lexoffice.io/v1/voucherlist?page=0&voucherType=salesinvoice&voucherStatus=any&voucherNumber={newInvoice.VoucherNumber}");
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(responseContent);
            int? totalElements = ((int)jsonObject["totalElements"]);
            return totalElements > 0;
        }
    }
}
