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
                contactId = voucher.ContactId,
                remark = "1.800 CoflCoins"
            };
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, serializerSettings);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.PostAsync("https://api.lexoffice.io/v1/vouchers", content);
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

    public async Task<string> createCustomerContact(string name, string lastname, string email)
    {

        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                person = new
                {
                    firstName = name,
                    lastName = lastname,
                },
                version = 0,
                roles = new
                {
                    customer = new { }
                },
                emailAddresses = new
                {
                    @private = new string[] { email }
                }
            };
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, serializerSettings);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.PostAsync("https://api.lexoffice.io/v1/contacts", content);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(responseContent);
            return jsonObject["id"].ToString();
        }
    }

    public async Task<string?> findCustomerContact(string email)
    {
        using (HttpClient client = new HttpClient())
        {
            string? token = config["LEX_OFFICE_TOKEN"];
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.GetAsync($"https://api.lexoffice.io/v1/contacts?email={email}");
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(responseContent);
            int? totalElements = ((int)jsonObject["numberOfElements"]);
            if (totalElements > 0)
            {
                return jsonObject["content"][0]["id"].ToString();
            }
            else
            {
                return null;
            }

        }
    }
}
