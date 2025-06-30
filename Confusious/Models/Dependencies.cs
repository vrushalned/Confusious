namespace Confusious.Models
{
    public class Dependencies
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsInternal { get; set; }
        public bool Found { get; set; }
        public string Source { get; set; }
        public bool IsVulnerable { get; set; }
    }
}
