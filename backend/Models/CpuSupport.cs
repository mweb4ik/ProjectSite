namespace PcComponentsApi.Models;

public class CpuSupport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string CpuId { get; set; } = string.Empty;
    public Component? Cpu { get; set; }

    public string BiosVersionId { get; set; } = string.Empty;
    public BiosVersion? BiosVersion { get; set; }

    public bool IsSupported { get; set; }
}
public class CpuCheckRequest
{
    public string CpuId { get; set; } = string.Empty;
    public string BiosId  { get; set; } = string.Empty;
}

