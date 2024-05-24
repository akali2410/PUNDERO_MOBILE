using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Vehicle
{
    public class PunderoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PunderoApiService(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
        }

        public async Task<List<InvoiceDto>> GetApprovedInvoicesForDriverAsync(int driverId)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"/api/Invoice/GetApprovedInvoicesForDriver/{driverId}");
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<InvoiceDto>>(jsonResponse);
        }

        public async Task<List<InvoiceDto>> GetInTransitInvoicesForDriverAsync(int driverId)
        {
            var response = await _httpClient.GetAsync(_baseUrl + $"/api/Invoice/GetInTransitInvoicesForDriver/{driverId}");
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<InvoiceDto>>(jsonResponse);
        }

        public async Task UpdateInvoiceStatusAsync(int invoiceId, int statusId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(statusId), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(_baseUrl + $"/api/Invoice/UpdateInvoiceStatus/{invoiceId}/status", content);
            response.EnsureSuccessStatusCode();
        }

        public class InvoiceDto
        {
            public int IdInvoice { get; set; }
            public int? IdDriver { get; set; } // Make it nullable
            public int IdStatus { get; set; }
            public string StoreName { get; set; }
            public string WarehouseName { get; set; }
            public DateTime IssueDate { get; set; }
        }
    }
}
