namespace Confusious.Models
{
    public class Dependencies
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsInternal { get; set; }
        public bool Found { get; set; }
    }
}
