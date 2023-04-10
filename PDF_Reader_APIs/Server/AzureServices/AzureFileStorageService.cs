using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using PDF_Reader_APIs.Shared.Entities;


namespace blazorTestApp.Server.Helpers
{
    public class AzureFileStorageService : IAzureFileStorageService
    {
        private readonly string ConnectionString;
        
        public AzureFileStorageService(IConfiguration Config)
        {
            ConnectionString = Config.GetConnectionString("AzureConnectionStorage");
        }
        
        public async Task<string> SaveFile(byte[] Content, string extention, string ContainerName)
        {
            var Client = new BlobContainerClient(ConnectionString, ContainerName); 
            await Client.CreateIfNotExistsAsync();
            Client.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            var FileName = $"{Guid.NewGuid()}{extention}";
            var blob = Client.GetBlobClient(FileName);
            using (var ms = new MemoryStream(Content))
            {
                await blob.UploadAsync(ms);
            }
            return blob.Uri.ToString();
        }
        
        public async Task DeleteFile(string ContainerName, string FileRoute)
        {
            if(!string.IsNullOrEmpty(FileRoute))
            {
                var Client = new BlobContainerClient(ConnectionString, ContainerName);
                var blob = Client.GetBlobClient(Path.GetFileName(FileRoute));
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task<string> EditFile(byte[] Content, string extention, string ContainerName, string FileRoute)
        {
            await DeleteFile(ContainerName, FileRoute);
            return await SaveFile(Content, extention, ContainerName);
        }
    }
}