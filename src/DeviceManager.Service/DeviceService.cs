using System.Text.RegularExpressions;
using DeviceManager.Entities;
using DeviceManager.Repository;

namespace DeviceManager.Logic;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(IDeviceRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<Device> GetAll() => _repository.GetAll();

    public Device? GetById(string id) => _repository.GetById(id);

    public bool Create(Device device)
    {
        string? validation = ValidateDevice(device);
        if (validation != null)
        {
            throw new ArgumentException(validation);
        }

        if (string.IsNullOrEmpty(device.Id))
        {
            var type = device switch
            {
                Smartwatch => "smartwatch",
                PersonalComputer => "personalcomputer",
                Embedded => "embedded",
                _ => throw new ArgumentException("Unknown device type")
            };

            device.Id = _repository.GenerateNextId(type);
        }

        var success = _repository.Create(device);
        if (!success)
            Console.WriteLine("Repository failed to create device.");

        return success;
    }

    public bool Update(Device device)
    {
        string? validation = ValidateDevice(device);
        if (validation != null)
            throw new ArgumentException(validation);

        return _repository.Update(device);
    }

    public bool Delete(string id, byte[] rowVersion)
    {
        return _repository.Delete(id, rowVersion);
    }

    private string? ValidateDevice(Device device)
    {
        if (device is PersonalComputer pc)
        {
            if (string.IsNullOrWhiteSpace(pc.OperatingSystem))
            {
                return "Cannot turn on PersonalComputer without operating system.";
            }
        }
        else if (device is Smartwatch sw)
        {
            if (sw.BatteryLevel < 0 || sw.BatteryLevel > 100)
            {
                return "Battery level must be between 0 and 100.";
            }
            if (sw.BatteryLevel < 11)
            {
                return "Battery level too low to turn on (must be at least 11%).";
            }
        }
        else if (device is Embedded embedded)
        {
            if (!IsValidIp(embedded.IpAddress))
            {
                return "Invalid IP address format.";
            }

            if (!embedded.NetworkName.Contains("MD Ltd."))
            {
                return "Network name must contain 'MD Ltd.'.";
            }
        }

        return null;
    }

    private bool IsValidIp(string ip)
    {
        var regex = new Regex(@"^(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}$");
        return regex.IsMatch(ip);
    }
    
}