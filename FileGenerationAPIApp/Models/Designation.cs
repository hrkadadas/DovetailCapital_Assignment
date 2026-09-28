namespace FileGenerationAPIApp.Models
{
    public class Designation
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
