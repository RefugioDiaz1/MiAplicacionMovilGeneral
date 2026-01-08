using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MiAppMovil.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://fordsureste.mx/WSRefacciones/WebServiceGP?wsdl") // Cambia aquí por la URL base de tu API
            };
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return data;
            }
            else
            {
                throw new Exception($"Error en la llamada: {response.StatusCode}");
            }
        }

        // Puedes agregar métodos para POST, PUT, DELETE aquí
    }
}
