using System;
using System.Collections.Generic;
using System.Threading;

namespace LaoHR.Bridge.Service.Services;

/// <summary>
/// Represents a single ZKTeco device connection using COM Interop (Late Binding)
/// </summary>
public class ZkDevice : IDisposable
{
    private dynamic? _zkDevice;
    private bool _isConnected = false;
    private readonly int _machineNumber = 1;
    private bool _disposed = false;

    public string Name { get; }
    public string IP { get; }
    public int Port { get; }
    public bool IsConnected => _isConnected;
    public string SerialNumber { get; private set; } = "";

    // Events
    public event Action<ZkDevice, AttendanceEventArgs>? OnAttendance;
    public event Action<ZkDevice>? OnFingerPlaced;
    public event Action<ZkDevice>? OnDisconnected;

    public ZkDevice(string name, string ip, int port = 4370)
    {
        Name = name;
        IP = ip;
        Port = port;
    }

    public bool Connect()
    {
        try
        {
            Disconnect();

            // Create COM object dynamically
            Type? zkType = Type.GetTypeFromProgID("zkemkeeper.ZKEM.1") ?? Type.GetTypeFromProgID("zkemkeeper.ZKEM");
            
            if (zkType == null)
            {
                Console.WriteLine("❌ ZKEM COM Object not found. Please register zkemkeeper.dll.");
                return false;
            }

            _zkDevice = Activator.CreateInstance(zkType);

            if (_zkDevice == null) return false;

            // Connect to device
            _isConnected = _zkDevice.Connect_Net(IP, Port);

            if (_isConnected)
            {
                _zkDevice.GetSerialNumber(_machineNumber, out string serial);
                SerialNumber = serial ?? "";

                RegisterEvents();

                // Enable real-time monitoring
                if (_zkDevice.RegEvent(_machineNumber, 65535))
                {
                    // Success
                }
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to {IP}: {ex.Message}");
            return false;
        }
    }

    public void Disconnect()
    {
        if (_zkDevice != null)
        {
            try { _zkDevice.Disconnect(); } catch { }
            try { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_zkDevice); } catch { }
            _zkDevice = null;
        }
        _isConnected = false;
    }

    public bool Reconnect()
    {
        Disconnect();
        Thread.Sleep(1000);
        return Connect();
    }

    private void RegisterEvents()
    {
        if (_zkDevice == null) return;

        try
        {
            // OnAttTransactionEx
            _zkDevice.OnAttTransactionEx += new Action<string, int, int, int, int, int, int, int, int, int, int>(
                (enrollNumber, isInValid, attState, verifyMethod, year, month, day, hour, minute, second, workCode) =>
                {
                    var args = new AttendanceEventArgs
                    {
                        EnrollNumber = enrollNumber,
                        EventTime = new DateTime(year, month, day, hour, minute, second),
                        DeviceName = Name,
                        DeviceIP = IP
                    };
                    OnAttendance?.Invoke(this, args);
                });

            // OnDisConnected
            _zkDevice.OnDisConnected += new Action(() =>
            {
                _isConnected = false;
                OnDisconnected?.Invoke(this);
            });
            
            // OnFinger
            _zkDevice.OnFinger += new Action(() =>
            {
                OnFingerPlaced?.Invoke(this);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering events: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Disconnect();
    }

    public List<ZkUserInfo> GetAllUsers()
    {
        var users = new List<ZkUserInfo>();
        if (!_isConnected || _zkDevice == null) return users;

        try
        {
            _zkDevice.ReadAllUserID(_machineNumber);
            _zkDevice.ReadAllTemplate(_machineNumber);

            string enrollNumber = "";
            string name = "";
            string password = "";
            int privilege = 0;
            bool enabled = false;

            while (_zkDevice.SSR_GetAllUserInfo(_machineNumber, out enrollNumber, out name, out password, out privilege, out enabled))
            {
                // Clean strings
                int nullIdx = name.IndexOf('\0');
                if (nullIdx >= 0) name = name.Substring(0, nullIdx);
                
                users.Add(new ZkUserInfo
                {
                    EnrollNumber = enrollNumber,
                    Name = name,
                    Password = password,
                    Privilege = privilege,
                    Enabled = enabled
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting users: {ex.Message}");
        }
        return users;
    }

    // Existing event arguments
}

public class ZkUserInfo
{
    public string EnrollNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Privilege { get; set; }
    public bool Enabled { get; set; }
}

public class AttendanceEventArgs
{
    public string EnrollNumber { get; set; } = "";
    public DateTime EventTime { get; set; }
    public string DeviceName { get; set; } = "";
    public string DeviceIP { get; set; } = "";
}
