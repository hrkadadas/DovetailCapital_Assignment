using FileGenerationAPIApp.Models;
using Microsoft.AspNetCore.Http;

namespace FileGenerationAPIApp.Services
{
    public interface IDocumentGeneratorService
    {
        DocumentRequestValidationResult ValidateDocumentRequest(DocumentRequest? request);
        Task<byte[]> GenerateZipAsync(IFormFile template, List<Dictionary<string,string>> datasets, CancellationToken cancellationToken = default);
    }

}
