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

    public async Task createLexOfficeInvoice(Invoice invoice)
    {
        bool doesInvoiceAlreadyExist = await doesInvoiceAlreadyExists(invoice);
        if (doesInvoiceAlreadyExist)
        {
            return;
        }

        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                archived = false,
                voucherDate = invoice.VoucherDate.ToString("2023-07-12T16:50:55.853+02:00"),
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
                    shippingDate = invoice.VoucherDate.ToString("2023-07-12T16:50:55.853+02:00"),
                    shippingType = "service"
                },
                paymentConditions = new
                {
                    paymentTermLabel = "Bezahlt, rein netto",
                    paymentTermDuration = 0
                }
            };
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, serializerSettings);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.PostAsync("https://api.lexoffice.io/v1/invoices", content);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();
        }
    }

    public async Task<IEnumerable<VoucherSearchResponse>> getInvoicesForVoucherDay(DateTime timestamp)
    {
        string voucherDay = timestamp.ToString("yyyy-MM-dd");
        using (HttpClient client = new HttpClient())
        {
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            bool isLast = false;
            int currentPage = 0;
            IEnumerable<VoucherSearchResponse> invoices = new List<VoucherSearchResponse>();
            while (!isLast)
            {
                HttpResponseMessage response = await client.GetAsync($"https://api.lexoffice.io/v1/voucherlist?page={currentPage}&voucherType=invoice&voucherStatus=any&voucherDateFrom={voucherDay}&voucherDateTo={voucherDay}");
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                JObject jsonObject = JObject.Parse(responseContent);
                isLast = jsonObject["last"]?.ToObject<bool>() ?? true;
                invoices = invoices.Concat(JsonConvert.DeserializeObject<List<VoucherSearchResponse>>(jsonObject["content"].ToString()));
            }

            return invoices;
        }
    }

    public async Task<bool> doesInvoiceAlreadyExists(Invoice newInvoice)
    {
        string newInvoiceTransactionId = ((TextInvoiceItem)newInvoice.Items.Single(item => item.Type == "text" && item.Name.Equals("Transaction-ID"))).Description;
        IEnumerable<VoucherSearchResponse> vouchers = await getInvoicesForVoucherDay(newInvoice.VoucherDate);
        vouchers = vouchers.Where(voucher =>
        {
            return voucher.ContactName == newInvoice.Address.Name;
        });
        using (HttpClient client = new HttpClient())
        {
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            foreach (VoucherSearchResponse voucher in vouchers)
            {
                HttpResponseMessage response = await client.GetAsync($"https://api.lexoffice.io/v1/invoices/{voucher.Id}");
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                JObject jsonObject = JObject.Parse(responseContent);
                string? transactionId = jsonObject["lineItems"]?.Children().Where(item => item["type"]?.ToString() == "text").Single(e => e["name"]?.ToString() == "Transaction-ID")["description"]?.ToString();
                if (transactionId?.Equals(newInvoiceTransactionId) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
