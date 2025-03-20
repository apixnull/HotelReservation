using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HotelReservation.Services
{
    public class VerimailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public VerimailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Verimail:ApiKey"]
                ?? throw new ArgumentNullException(nameof(configuration), "Verimail API key is missing.");
        }

        /// <summary>
        /// Calls the Verimail API and returns the full response.
        /// </summary>
        public async Task<bool> IsValidEmailAsync(string email)
        {
            var url = $"https://api.verimail.io/v3/verify?key={_apiKey}&email={email}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Verimail API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            // ✅ Directly check "deliverable" without extra models
            return doc.RootElement.GetProperty("deliverable").GetBoolean();
        }
    }
}
