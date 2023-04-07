using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PDF_Reader_APIs.Shared.Entities;


namespace blazorTestApp.Client.Classes_FE
{
    public class HttpService
    {
        private readonly HttpClient httpClient;
        private JsonSerializerOptions defaultJsonSerializerOptions => new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
        public HttpService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HttpResponse<T>> GET<T>(string url)
        {
            var response = await httpClient.GetAsync(url);
            if(response.IsSuccessStatusCode)
            {
                var DeserializedResponse = await Deserialize<T>(response, defaultJsonSerializerOptions);
                return new HttpResponse<T>(DeserializedResponse, response, true);
            }
            else
            {
                return new HttpResponse<T>(default, response, false);
            }
        }
        
        public async Task<HttpResponse<object>> POST<T>(string url, T data)
        {
            var JsonData = JsonSerializer.Serialize(data);
            var StringContent = new StringContent(JsonData, Encoding.UTF8, "application/json");
            var ResponseMessage = await httpClient.PostAsync(url, StringContent);
            return new HttpResponse<object>(null, ResponseMessage, ResponseMessage.IsSuccessStatusCode);
        }

        public async Task<HttpResponse<object>> PostGeneric<T, TResponse>(string url, T data)
        {
            var JsonData = JsonSerializer.Serialize(data);
            var StringContent = new StringContent(JsonData, Encoding.UTF8, "application/json");
            var ResponseMessage = await httpClient.PostAsync(url, StringContent);
            if(ResponseMessage.IsSuccessStatusCode)
            {
                var responseDeserializer = await Deserialize<TResponse>(ResponseMessage, defaultJsonSerializerOptions);
                return new HttpResponse<object>(responseDeserializer, ResponseMessage, true);
            }
            else
            {
                return new HttpResponse<object>(default, ResponseMessage, false);
            }
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage responseMessage, JsonSerializerOptions options)
        {
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseString, options);
        }
    }
}