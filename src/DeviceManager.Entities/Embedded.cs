namespace DeviceManager.Entities;

public class Embedded : Device
{
    public string IpAddress { get; set; } = default!;
    public string NetworkName { get; set; } = default!;

    public Embedded() { }

    public Embedded(string id, string name, bool isEnabled,string ipAddress, string networkName, byte[] rowVersion)
        : base(id, name, isEnabled, rowVersion)
    {
        IpAddress = ipAddress;
        NetworkName = networkName;
    }
}