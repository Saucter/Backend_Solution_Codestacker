using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PDF_Reader_APIs.Shared.Entities;


namespace PDF_Reader_APIs.Server.AzureStorageServices
{
    public interface IAzureFileStorageService
    {
        Task<string> SaveFile(byte[] content, string extention, string Container);
        Task DeleteFile(string Contaienr, string Route);
        Task<string> EditFile(byte[] Content, string extention, string Container, string Route);
    }
}