namespace PcComponentsApi.Models;

public class Component
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public ComponentCategory Category { get; set; } 
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Specifications { get; set; } = string.Empty;
    public string Socket { get; set; } = string.Empty;
    public int PowerConsumption { get; set; } // в ваттах
}
public enum ComponentCategory
{
    Processor,
    Motherboard,
    Ram,
    Storage,
    Videocard,
    Cooling

}