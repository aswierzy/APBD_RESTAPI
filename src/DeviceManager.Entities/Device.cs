namespace DeviceManager.Entities;

public class Device
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsEnabled { get; set; }

    public Device() { }

    public Device(string id, string name, bool isEnabled)
    {
        Id = id;
        Name = name;
        IsEnabled = isEnabled;
    }
}