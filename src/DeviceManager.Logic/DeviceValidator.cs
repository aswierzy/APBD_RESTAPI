using System.Text.RegularExpressions;
using DeviceManager.Entities;

namespace DeviceManager.Logic;

public class DeviceValidator : IDeviceValidator
{
    public string? ValidateDevice(Device device)
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

    bool IsValidIp(string ip)
    {
        var regex = new Regex(@"^(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}$");
        return regex.IsMatch(ip);
    }
}