using DeviceManager.Entities;

namespace DeviceManager.Logic;

public interface IDeviceValidator
{
    string? ValidateDevice(Device device);
}