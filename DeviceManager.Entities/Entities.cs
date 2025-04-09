namespace DeviceManager.Entities;
public abstract class Device
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsEnabled { get; set; }

    protected Device() { }

    protected Device(string id, string name, bool isEnabled)
    {
        Id = id;
        Name = name;
        IsEnabled = isEnabled;
    }
}

public class PersonalComputer : Device
{
    public string? OperatingSystem { get; set; }

    public PersonalComputer() { }

    public PersonalComputer(string id, string name, bool isEnabled, string? operatingSystem)
        : base(id, name, isEnabled)
    {
        OperatingSystem = operatingSystem;
    }
}

public class Smartwatch : Device
{
    public int BatteryLevel { get; set; }

    public Smartwatch() { }

    public Smartwatch(string id, string name, bool isEnabled, int batteryLevel)
        : base(id, name, isEnabled)
    {
        BatteryLevel = batteryLevel;
    }
}

public class Embedded : Device
{
    public string IpAddress { get; set; } = default!;
    public string NetworkName { get; set; } = default!;

    public Embedded() { }

    public Embedded(string id, string name, bool isEnabled, string ipAddress, string networkName)
        : base(id, name, isEnabled)
    {
        IpAddress = ipAddress;
        NetworkName = networkName;
    }
}