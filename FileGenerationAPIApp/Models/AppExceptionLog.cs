namespace FileGenerationAPIApp.Models
{
    public class AppExceptionLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
        public string? StackTrace { get; set; }
        public string? Path { get; set; }
        public string? Level { get; set; }
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
