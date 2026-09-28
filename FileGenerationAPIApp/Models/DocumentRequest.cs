using Microsoft.AspNetCore.Http;

namespace FileGenerationAPIApp.Models
{
    // Model used to represent multipart/form-data for the document generation endpoint.
    public class DocumentRequest
    {
        public IFormFile? Template { get; set; }
        public string? DatasetsJson { get; set; }
    }
}
