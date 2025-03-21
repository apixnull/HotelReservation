using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HotelReservation.Services
{
    public class PayMongoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public PayMongoService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _secretKey = configuration["PayMongo:SecretKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_secretKey)));
        }

        public async Task<string?> CreatePaymentIntent(decimal amount)
        {
            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = (int)(amount * 100), // Convert to cents
                        payment_method_allowed = new[] { "gcash" },
                        currency = "PHP"
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.paymongo.com/v1/payment_intents", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode ? responseString : null;
        }
    }
}
