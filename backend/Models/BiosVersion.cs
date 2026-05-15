namespace PcComponentsApi.Models;
public class BiosVersion
{   public string Id  { get; set; } = Guid.NewGuid().ToString();
     public  string  MotherboardId { get; set; } = string.Empty;
      public  string Version { get; set; }  = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public  string Description { get; set; }  = string.Empty;
        public double Stability { get; set; }
        public bool IsBeta {get;set;}
        public Motherboard? Motherboard { get; set; }
        public ICollection<CpuSupport>? SupportedCpus { get; set; }
}
public class UpdateCheckRequest
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string TargetVersion { get; set; } = string.Empty;
}