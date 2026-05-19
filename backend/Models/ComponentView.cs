namespace PcComponentsApi.Models;

public class ComponentView
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public string ComponentId { get; set; } = string.Empty;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}