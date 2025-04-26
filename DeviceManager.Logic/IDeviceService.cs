namespace DeviceManager.Logic;
using DeviceManager.Entities;

public interface IDeviceService
{
    IEnumerable<Device> GetAll();
    Device? GetById(string id);
    bool Create(Device device);
    bool Update(Device device);
    bool Delete(string id);
}