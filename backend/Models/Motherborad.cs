namespace PcComponentsApi.Models;

public class Motherboard
{
     public string Id { get; set; } = Guid.NewGuid().ToString();
     public  string Name { get; set; } = string.Empty;
    public  string Socket { get; set; } = string.Empty;
    public  string Chipset { get; set; } = string.Empty;
      
}

