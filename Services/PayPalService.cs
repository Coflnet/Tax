using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Tax;

public class PayPalService
{
    public IConfiguration config;
    public string accessToken;
    public DateTime expiration;

    public PayPalService(IConfiguration config)
    {
        this.config = config;
    }

    public async Task updateAccessToken()
    {
        using (HttpClient client = new HttpClient())
        {
            HttpContent content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            var authenticationString = $"{config["PAYPAL:ID"]}:{config["PAYPAL:SECRET"]}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic ${base64EncodedAuthenticationString}");

            HttpResponseMessage response = await client.PostAsync("https://api-m.paypal.com/v1/oauth2/token", content);
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(responseContent);
            accessToken = jsonObject["access_token"].ToString();
            expiration = new DateTime(DateTime.Now.Ticks + ((long)jsonObject["expires_in"]));
        }
    }

    public Task updateAccessTokenIfNeeded()
    {
        if (accessToken is null || expiration < DateTime.Now)
        {
            return updateAccessToken();
        }
        return Task.CompletedTask;
    }

    public async Task<decimal> getPayPalFeeForTransaction(string paymentID)
    {
        await updateAccessTokenIfNeeded();
        using (HttpClient client = new HttpClient())
        {
            HttpContent content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/json");
            var authenticationString = $"{config["PAYPAL:ID"]}:{config["PAYPAL:SECRET"]}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic ${base64EncodedAuthenticationString}");

            HttpResponseMessage response = await client.GetAsync($"https://api-m.paypal.com/v2/payments/captures/{paymentID}");
            response.EnsureSuccessStatusCode();
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(responseContent);
            string value = jsonObject["seller_receivable_breakdown"]["paypal_fee"]["value"].ToString();
            return Convert.ToDecimal(value, new CultureInfo("en-US"));
        }
    }
}
