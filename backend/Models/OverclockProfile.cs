namespace PcComponentsApi.Models;

public class OverclockProfile
{
     public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string CpuId { get; set; } = string.Empty;
    public double Frequency { get; set; } 
    public  double Voltage { get; set; } 
    public double Temperature { get; set; } 
    public double Stability { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
      
}

public class OverclockRequest
{
    public double Frequency { get; set; }
    public double Voltage { get; set; }
}