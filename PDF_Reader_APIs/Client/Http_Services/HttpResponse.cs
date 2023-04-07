using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PDF_Reader_APIs.Shared.Entities;


namespace blazorTestApp.Client.Classes_FE
{
    public class HttpResponse<T>
    {
        public HttpResponse( T Response, HttpResponseMessage ResponseMessage, bool Success)
        {
            this.Success = Success;
            this.Response = Response;
            this.ResponseMessage = ResponseMessage;
        }
        public bool Success {get; set;}
        public T Response {get; set;}
        public HttpResponseMessage ResponseMessage {get; set;}

        public async Task<string> GetBodyString()
        {
            return await ResponseMessage.Content.ReadAsStringAsync();
        }
    }
}