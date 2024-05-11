using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;

//using ThreadNetwork;

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
              
              var url = _baseUrl + $"/api/Invoice/GetInvoices"; // Replace with your actual API endpoint

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

    
        

    } 

    
}


