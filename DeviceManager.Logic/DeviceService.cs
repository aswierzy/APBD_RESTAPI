namespace DeviceManager.Logic;
using DeviceManager.Entities;

public class DeviceService : IDeviceService
{
    private static readonly List<Device> _devices = new()
    {
        new PersonalComputer("P-1", "My PC", true, "Windows 11"),
        new Smartwatch("SW-1", "My Watch", false, 75),
        new Embedded("ED-1", "My Embedded", true, "192.168.0.10", "MD Ltd. IoT Lab")
    };

    public IEnumerable<Device> GetAll() => _devices;

    public Device? GetById(string id) =>
        _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public void Create(Device device)
    {
        if (_devices.Any(d => d.Id.Equals(device.Id, StringComparison.OrdinalIgnoreCase)))
            throw new Exception($"Device with id={device.Id} already exists.");
        
        _devices.Add(device);
    }

    public void Update(string id, Device device)
    {
        var index = _devices.FindIndex(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index == -1)
            throw new Exception($"Device with id={id} not found.");

        device.Id = id;
        _devices[index] = device;
    }

    public void Delete(string id)
    {
        var device = _devices.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (device is null)
            throw new Exception($"Device with id={id} not found.");

        _devices.Remove(device);
    }
}