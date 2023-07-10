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

    public async Task createLexOfficeInvoice(Invoice invoice)
    {
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                archived = false,
                voucherDate = invoice.VoucherDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"),
                address = invoice.Address,
                lineItems = invoice.Items,
                totalPrice = new
                {
                    currency = invoice.Currency ?? "EUR"
                },
                taxConditions = new
                {
                    taxType = "gross"
                },
                shippingConditions = new
                {
                    shippingDate = invoice.VoucherDate.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK"),
                    shippingType = "service"
                },
                paymentConditions = new
                {
                    paymentTermLabel = "Bezahlt, rein netto",
                    paymentTermDuration = 0
                },
                title = invoice.Title ?? "Rechnung",
                introduction = invoice.Introduction ?? "Ihre bestellten Positionen stellen wir Ihnen hiermit in Rechnung",
                remark = invoice.Remark ?? "Vielen Dank für Ihren Einkauf"
            };
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, serializerSettings);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            string token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.PostAsync("https://api.lexoffice.io/v1/invoices", content);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<List<VoucherSearchResponse>> getLastInvoices()
    {
        using (HttpClient client = new HttpClient())
        {
            string token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.GetAsync("https://api.lexoffice.io/v1/voucherlist?voucherType=invoice&voucherStatus=any&sort=createdDate,DESC");
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();

            JObject jsonObject = JObject.Parse(responseContent);
            List<VoucherSearchResponse> invoices = JsonConvert.DeserializeObject<List<VoucherSearchResponse>>(jsonObject["content"].ToString());
            return invoices;
        }
    }
}
