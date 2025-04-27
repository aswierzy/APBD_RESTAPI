namespace DeviceManager.Logic;
using Microsoft.Data.SqlClient;
using DeviceManager.Entities;


public class DeviceService : IDeviceService
{
    
    private string _connectionString;

    public DeviceService(string connectionString)
    {
        _connectionString = connectionString;
    }


    public IEnumerable<Device> GetAll()
    {
        List<Device> devices = [];
        const string queryString = "SELECT * FROM Device";
        using (SqlConnection connection = new(_connectionString))
        {
            SqlCommand command = new(queryString, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var deviceRow = new Device
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsEnabled = reader.GetBoolean(2)
                        };
                        devices.Add(deviceRow);
                    }
                }
            }
            finally
            {
                reader.Close();
            }
        }
        return devices;
    }

    public Device? GetById(string id)
{
    using (SqlConnection connection = new(_connectionString))
    {
        connection.Open();

        const string deviceQuery = "SELECT * FROM Device WHERE Id = @Id";
        SqlCommand deviceCommand = new(deviceQuery, connection);
        deviceCommand.Parameters.AddWithValue("@Id", id);

        using (SqlDataReader deviceReader = deviceCommand.ExecuteReader())
        {
            if (!deviceReader.Read())
            {
                return null;
            }

            string name = deviceReader["Name"].ToString()!;
            bool isEnabled = (bool)deviceReader["IsEnabled"];
            deviceReader.Close();

            const string pcQuery = "SELECT * FROM PersonalComputer WHERE DeviceId = @Id";
            SqlCommand pcCommand = new(pcQuery, connection);
            pcCommand.Parameters.AddWithValue("@Id", id);

            using (SqlDataReader pcReader = pcCommand.ExecuteReader())
            {
                if (pcReader.Read())
                {
                    string? operatingSystem = pcReader["OperationSystem"] as string;
                    return new PersonalComputer(id, name, isEnabled, operatingSystem);
                }
            }

            const string swQuery = "SELECT * FROM Smartwatch WHERE DeviceId = @Id";
            SqlCommand swCommand = new(swQuery, connection);
            swCommand.Parameters.AddWithValue("@Id", id);

            using (SqlDataReader swReader = swCommand.ExecuteReader())
            {
                if (swReader.Read())
                {
                    int battery = swReader["BatteryPercentage"] != DBNull.Value
                        ? Convert.ToInt32(swReader["BatteryPercentage"])
                        : 0;

                    return new Smartwatch(id, name, isEnabled, battery);
                }
            }

            const string edQuery = "SELECT * FROM Embedded WHERE DeviceId = @Id";
            SqlCommand edCommand = new(edQuery, connection);
            edCommand.Parameters.AddWithValue("@Id", id);

            using (SqlDataReader edReader = edCommand.ExecuteReader())
            {
                if (edReader.Read())
                {
                    string ipAddress = edReader["IpAddress"] as string ?? "0.0.0.0";
                    string networkName = edReader["NetworkName"] as string ?? "Unknown";

                    return new Embedded(id, name, isEnabled, ipAddress, networkName);
                }
            }
        }
    }

    return null;
}
    
   public bool Create(Device device)
{
    using (SqlConnection connection = new(_connectionString))
    {
        connection.Open();
        string devicePrefix = device switch
        {
            PersonalComputer => "P",
            Smartwatch => "SW",
            Embedded => "ED",
            _ => throw new ArgumentException("Unsupported device type.")
        };

        device.Id = GenerateNewId(connection,devicePrefix);
        try
        {
            const string deviceQuery = "INSERT INTO Device (Id, Name, IsEnabled) VALUES (@Id, @Name, @IsEnabled)";
            SqlCommand deviceCommand = new(deviceQuery, connection);
            deviceCommand.Parameters.AddWithValue("@Id", device.Id);
            deviceCommand.Parameters.AddWithValue("@Name", device.Name);
            deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
            deviceCommand.ExecuteNonQuery();

            switch (device)
            {
                case PersonalComputer pc:
                    const string pcQuery = "INSERT INTO PersonalComputer (OperationSystem, DeviceId) VALUES (@OS, @DeviceId)";
                    SqlCommand pcCommand = new(pcQuery, connection);
                    pcCommand.Parameters.AddWithValue("@OS", pc.OperatingSystem ?? (object)DBNull.Value);
                    pcCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                    pcCommand.ExecuteNonQuery();
                    break;

                case Smartwatch sw:
                    const string swQuery = "INSERT INTO Smartwatch (BatteryPercentage, DeviceId) VALUES (@Battery, @DeviceId)";
                    SqlCommand swCommand = new(swQuery, connection);
                    swCommand.Parameters.AddWithValue("@Battery", sw.BatteryLevel);
                    swCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                    swCommand.ExecuteNonQuery();
                    break;

                case Embedded ed:
                    const string edQuery = "INSERT INTO Embedded (IpAddress, NetworkName, DeviceId) VALUES (@Ip, @Network)";
                    SqlCommand edCommand = new(edQuery, connection);
                    edCommand.Parameters.AddWithValue("@Ip", ed.IpAddress ?? (object)DBNull.Value);
                    edCommand.Parameters.AddWithValue("@Network", ed.NetworkName ?? (object)DBNull.Value);
                    edCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                    edCommand.ExecuteNonQuery();
                    break;

                default:
                    throw new ArgumentException("Unsupported device type.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Error: {ex.Message}");
            return false;
        }
    }
}
    
    public bool Update(Device device)
{
    using (SqlConnection connection = new(_connectionString))
    {
        connection.Open();

        try
        {
            const string deviceQuery = "UPDATE Device SET Name = @Name, IsEnabled = @IsEnabled WHERE Id = @Id";
            SqlCommand deviceCommand = new(deviceQuery, connection);
            deviceCommand.Parameters.AddWithValue("@Id", device.Id);
            deviceCommand.Parameters.AddWithValue("@Name", device.Name);
            deviceCommand.Parameters.AddWithValue("@IsEnabled", device.IsEnabled);
            int rowsAffected = deviceCommand.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                return false;
            }

            switch (device)
            {
                case PersonalComputer pc:
                    const string pcQuery = "UPDATE PersonalComputer SET OperationSystem = @OS WHERE DeviceId = @DeviceId";
                    SqlCommand pcCommand = new(pcQuery, connection);
                    pcCommand.Parameters.AddWithValue("@OS", pc.OperatingSystem ?? (object)DBNull.Value);
                    pcCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                    pcCommand.ExecuteNonQuery();
                    break;

                case Smartwatch sw:
                    const string swQuery = "UPDATE Smartwatch SET BatteryPercentage = @Battery WHERE DeviceId = @DeviceId";
                    SqlCommand swCommand = new(swQuery, connection);
                    swCommand.Parameters.AddWithValue("@Battery", sw.BatteryLevel);
                    swCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                    swCommand.ExecuteNonQuery();
                    break;

                case Embedded ed:
                    const string edQuery = "UPDATE Embedded SET IpAddress = @Ip, NetworkName = @Network WHERE DeviceId = @DeviceId";
                    SqlCommand edCommand = new(edQuery, connection);
                    edCommand.Parameters.AddWithValue("@Ip", ed.IpAddress ?? (object)DBNull.Value);
                    edCommand.Parameters.AddWithValue("@Network", ed.NetworkName ?? (object)DBNull.Value);
                    edCommand.Parameters.AddWithValue("@DeviceId", device.Id);
                    edCommand.ExecuteNonQuery();
                    break;

                default:
                    throw new ArgumentException("Unsupported device type.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Error during update: {ex.Message}");
            return false;
        }
    }
}
    
    //     FOREIGN KEY (DeviceId) REFERENCES Device(Id) ON DELETE CASCADE
    // following the above table creation, by deleting a device from the device table,
    // the related device is deleted as well form its type table and it works as expected
    public bool Delete(string id)
    {
        using (SqlConnection connection = new(_connectionString))
        {
            connection.Open();

            try
            {
                const string deleteQuery = "DELETE FROM Device WHERE Id = @Id";

                SqlCommand command = new(deleteQuery, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error during delete: {ex.Message}");
                return false;
            }
        }
    }
    
    private string GenerateNewId(SqlConnection connection, string devicePrefix)
    {
        string query = "SELECT MAX(Id) FROM Device WHERE Id LIKE @Prefix + '%'";
        SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@Prefix", devicePrefix);

        var lastIdObj = command.ExecuteScalar();
        if (lastIdObj != DBNull.Value && lastIdObj != null)
        {
            string lastId = lastIdObj.ToString()!;
            int lastNumber = int.Parse(lastId.Split('-')[1]);
            return $"{devicePrefix}-{lastNumber + 1}";
        }
        else
        {
            return $"{devicePrefix}-1";
        }
    }
    
}