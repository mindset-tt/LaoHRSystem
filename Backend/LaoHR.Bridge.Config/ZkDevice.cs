using System;
using System.Collections.Generic;
using System.Threading;

namespace LaoHR.Bridge.Config;

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
                // Console.WriteLine("❌ ZKEM COM Object not found. Please register zkemkeeper.dll.");
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
                return true;
            }

            return false;
        }
        catch (Exception)
        {
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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Disconnect();
    }
}
