using System.Data;
using System.Data.SqlClient;
using DeviceManager.Entities;
using Microsoft.Data.SqlClient;

namespace DeviceManager.Repository;

public class DeviceRepository : IDeviceRepository
{
    private readonly string _connectionString;

    public DeviceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Device> GetAll()
    {
        var devices = new List<Device>();
        const string query = "SELECT Id, Name, IsEnabled, RowVersion FROM Device";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(query, connection);
        connection.Open();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            devices.Add(new Device(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetBoolean(2),
                (byte[])reader[3]

            ));
        }

        return devices;
    }

    public Device? GetById(string id)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        const string query = "SELECT Id, Name, IsEnabled, RowVersion FROM Device WHERE Id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        string name = reader.GetString(1);
        bool isEnabled = reader.GetBoolean(2);
        byte[] rowVersion = (byte[])reader[3];

        string deviceType = DetectDeviceType(connection, id);

        return deviceType switch
        {
            "Smartwatch" => GetSmartwatch(connection, id, name, isEnabled, rowVersion),
            "PersonalComputer" => GetPC(connection, id, name, isEnabled, rowVersion),
            "Embedded" => GetEmbedded(connection, id, name, isEnabled, rowVersion),
            _ => null
        };
    }

    private static string DetectDeviceType(SqlConnection connection, string id)
    {
        const string query = @"SELECT CASE
            WHEN EXISTS (SELECT 1 FROM Smartwatch WHERE DeviceId = @Id) THEN 'Smartwatch'
            WHEN EXISTS (SELECT 1 FROM PersonalComputer WHERE DeviceId = @Id) THEN 'PersonalComputer'
            WHEN EXISTS (SELECT 1 FROM Embedded WHERE DeviceId = @Id) THEN 'Embedded'
            ELSE NULL END";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", id);
        return command.ExecuteScalar()?.ToString() ?? "";
    }

    private Smartwatch GetSmartwatch(SqlConnection conn, string id, string name, bool enabled, byte[] rowVersion)
    {
        const string q = "SELECT BatteryPercentage FROM Smartwatch WHERE DeviceId = @Id";
        using var cmd = new SqlCommand(q, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        int battery = Convert.ToInt32(cmd.ExecuteScalar());
        return new Smartwatch(id, name, enabled, battery,rowVersion);
    }

    private PersonalComputer GetPC(SqlConnection conn, string id, string name, bool enabled, byte[] rowVersion)
    {
        const string q = "SELECT OperationSystem FROM PersonalComputer WHERE DeviceId = @Id";
        using var cmd = new SqlCommand(q, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        string? os = cmd.ExecuteScalar()?.ToString();
        return new PersonalComputer(id, name, enabled, os,rowVersion) { RowVersion = rowVersion };
    }

    private Embedded GetEmbedded(SqlConnection conn, string id, string name, bool enabled, byte[] rowVersion)
    {
        const string q = "SELECT IpAddress, NetworkName FROM Embedded WHERE DeviceId = @Id";
        using var cmd = new SqlCommand(q, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        reader.Read();
        return new Embedded(id, name, enabled, reader[0].ToString()!, reader[1].ToString()!,rowVersion) { RowVersion = rowVersion };
    }

    public bool Create(Device device)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var command = new SqlCommand { Connection = connection, CommandType = CommandType.StoredProcedure };

        try
        {
            switch (device)
            {
                case Smartwatch sw:
                    command.CommandText = "AddSmartwatch";
                    command.Parameters.AddWithValue("@DeviceId", sw.Id);
                    command.Parameters.AddWithValue("@Name", sw.Name);
                    command.Parameters.AddWithValue("@IsEnabled", sw.IsEnabled);
                    command.Parameters.AddWithValue("@BatteryPercentage", sw.BatteryLevel);
                    break;
                case PersonalComputer pc:
                    command.CommandText = "AddPersonalComputer";
                    command.Parameters.AddWithValue("@DeviceId", pc.Id);
                    command.Parameters.AddWithValue("@Name", pc.Name);
                    command.Parameters.AddWithValue("@IsEnabled", pc.IsEnabled);
                    command.Parameters.AddWithValue("@OperatingSystem", pc.OperatingSystem ?? (object)DBNull.Value);
                    break;
                case Embedded ed:
                    command.CommandText = "AddEmbedded";
                    command.Parameters.AddWithValue("@DeviceId", ed.Id);
                    command.Parameters.AddWithValue("@Name", ed.Name);
                    command.Parameters.AddWithValue("@IsEnabled", ed.IsEnabled);
                    command.Parameters.AddWithValue("@IpAddress", ed.IpAddress);
                    command.Parameters.AddWithValue("@NetworkName", ed.NetworkName);

                    break;
                default:
                    return false;
            }

            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool Update(Device device)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string updateQuery = @"UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled WHERE Id = @Id AND RowVersion = @RowVersion";

            using var command = new SqlCommand(updateQuery, connection, transaction);
            command.Parameters.AddWithValue("@Id", device.Id);
            command.Parameters.AddWithValue("@Name", device.Name);
            command.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
            command.Parameters.AddWithValue("@RowVersion", device.RowVersion);

            int affected = command.ExecuteNonQuery();
            if (affected == 0)
            {
                transaction.Rollback();
                return false;
            }
            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return false;
        }
    }

    public bool Delete(string id, byte[] rowVersion)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            string type = DetectDeviceType(connection, id);
            string deleteChild = type switch
            {
                "Smartwatch" => "DELETE FROM Smartwatch WHERE DeviceId = @Id",
                "PersonalComputer" => "DELETE FROM PersonalComputer WHERE DeviceId = @Id",
                "Embedded" => "DELETE FROM Embedded WHERE DeviceId = @Id",
                _ => throw new Exception("Unknown device type")
            };

            using var childCmd = new SqlCommand(deleteChild, connection, transaction);
            childCmd.Parameters.AddWithValue("@Id", id);
            childCmd.ExecuteNonQuery();
            
            const string deleteDevice = "DELETE FROM Device WHERE Id = @Id AND RowVersion = @RowVersion";
            using var deviceCmd = new SqlCommand(deleteDevice, connection, transaction);
            deviceCmd.Parameters.AddWithValue("@Id", id);
            deviceCmd.Parameters.AddWithValue("@RowVersion", rowVersion);

            int affected = deviceCmd.ExecuteNonQuery();
            if (affected == 0)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }
    
    public string GenerateNextId(string deviceType)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        string table = deviceType.ToLower() switch
        {
            "smartwatch" => "Smartwatch",
            "personalcomputer" => "PersonalComputer",
            "embedded" => "Embedded",
            _ => throw new ArgumentException("Invalid device type")
        };

        string prefix = deviceType.ToLower() switch
        {
            "smartwatch" => "SW-",
            "personalcomputer" => "PC-",
            "embedded" => "ED-",
            _ => ""
        };

        var query = $"SELECT COUNT(*) FROM {table}";
        using var command = new SqlCommand(query, connection);
        int count = (int)command.ExecuteScalar();

        return $"{prefix}{count + 1}";
    }
    
}
