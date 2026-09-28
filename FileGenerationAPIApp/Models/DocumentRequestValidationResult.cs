namespace FileGenerationAPIApp.Models
{
    public class DocumentRequestValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Dictionary<string, string>>? Datasets { get; set; }
    }
}
