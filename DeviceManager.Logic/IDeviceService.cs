namespace DeviceManager.Logic;
using DeviceManager.Entities;

public interface IDeviceService
{
    IEnumerable<Device> GetAll();
    Device? GetById(string id);
    void Create(Device device);
    void Update(string id, Device device);
    void Delete(string id);
}