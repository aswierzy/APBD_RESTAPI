namespace DeviceManager.Entities;

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