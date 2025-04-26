namespace DeviceManager.Entities;

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