using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;




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

        public async Task<List<Invoice>> GetInvoicesAsync()
        {

            var url = _baseUrl + $"/api/Invoice/GetInvoices";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var invoices = await response.Content.ReadFromJsonAsync<List<Invoice>>();

                return invoices;
            }
            catch (HttpRequestException ex)
            {

                // Handle HTTP request exceptions (e.g., network issues)
                Console.WriteLine($"Error getting invoices: {ex.Message}");

                throw; // Or return an empty list or a custom error object
            }
            catch (Exception ex)
            {
                // Handle other exceptions (e.g., JSON deserialization errors)
                Console.WriteLine($"Unexpected error getting invoices: {ex.Message}");
                throw; // Or return an empty list or a custom error object
            }
        }

        public class Invoice
        {
            public int IdInvoice { get; set; }  // Assuming "idInvoice" is the invoice identifier
            public DateTime IssueDate { get; set; }
            public int IdStore { get; set; }
            public int IdWarehouse { get; set; }
            public int IdStatus { get; set; }

            /* // Navigation properties can be ignored for now (if not used)
             public InvoiceStatus IdStatusNavigation { get; set; }  // Assuming InvoiceStatus class exists
             public Store IdStoreNavigation { get; set; }  // Assuming Store class exists
             public Warehouse IdWarehouseNavigation { get; set; }  // Assuming Warehouse class exists

             public List<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();  // Initialize empty list
         }

         // Assuming separate classes exist for nested objects (optional)
         public class InvoiceStatus
         {
             // ... properties for InvoiceStatus
         }

         public class Store
         {
             // ... properties for Store
         }

         public class Warehouse
         {
             // ... properties for Warehouse
         }

         public class InvoiceProduct
         {
             // ... properties for InvoiceProduct (likely related to products in the invoice)
         }*/


        }


    }
}

