using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using blazorTestApp.Client.Classes_FE;
using PDF_Reader_APIs.Shared.Entities;


namespace blazorTestApp.Client.Repositories
{
    public class PdfRepo
    {
        public HttpService httpService;
        public string url = "api/pdf";
        public PdfRepo(HttpService httpService)
        {
            this.httpService = httpService;
        }

        public async Task<object> AddBook(PDF pdf)
        {
            var response = await httpService.PostGeneric<PDF, int>(url, pdf);
            if(!response.Success)
            {
                throw new ApplicationException(await response.GetBodyString());
            }
            return response.Response;
        }

        public async Task<object> GetBooks()
        {
            var response = await httpService.GET<List<PDF>>(url);
            if(!response.Success)
            {
                throw new ApplicationException(await response.GetBodyString());
            }
            return response.Response;
        }
        
    }
}