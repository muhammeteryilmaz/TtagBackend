using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadAsync(IFormFile file, string containerName);
        Task DeleteAsync(string blobName, string containerName);
        Task<string> UpdateAsync(IFormFile file, string oldBlobName, string containerName);
    }
}