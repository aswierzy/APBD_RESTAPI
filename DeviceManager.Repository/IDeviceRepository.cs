using DeviceManager.Entities;

namespace DeviceManager.Repository;

public interface IDeviceRepository
{
    IEnumerable<Device> GetAll();
    Device? GetById(string id);
    bool Create(Device device);
    bool Update(Device device);
    bool Delete(string id,byte[] rowVersion);
    string GenerateNextId(string deviceType);
}