namespace PcComponentsApi.Models;
public class PCBuild
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ComponentsJson { get; set; } = string.Empty; // временно (потом   через связи)
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsCompatible { get; set; }
}