CREATE TABLE Device (
    Id VARCHAR(50) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    IsEnabled BIT NOT NULL
);

CREATE TABLE PersonalComputer (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OperationSystem VARCHAR(100),
    DeviceId VARCHAR(50) NOT NULL,
    FOREIGN KEY (DeviceId) REFERENCES Device(Id) ON DELETE CASCADE
);

CREATE TABLE Smartwatch (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BatteryPercentage INT,
    DeviceId VARCHAR(50) NOT NULL,
    FOREIGN KEY (DeviceId) REFERENCES Device(Id) ON DELETE CASCADE
);

CREATE TABLE Embedded (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IpAddress VARCHAR(50),
    NetworkName VARCHAR(100),
    DeviceId VARCHAR(50) NOT NULL,
    FOREIGN KEY (DeviceId) REFERENCES Device(Id) ON DELETE CASCADE
);


INSERT INTO Device (Id, Name, IsEnabled)
VALUES
    ('P-1', 'Asus rog', 1),
    ('SW-1', 'Apple Watch 5', 1),
    ('ED-1', 'Embedded1', 0);

INSERT INTO PersonalComputer (OperationSystem, DeviceId)
VALUES ('Windows 11', 'P-1');

INSERT INTO Smartwatch (BatteryPercentage, DeviceId)
VALUES (65, 'SW-1');

INSERT INTO Embedded (IpAddress, NetworkName, DeviceId)
VALUES ('192.168.0.101', 'MD Ltd Network', 'ED-1');