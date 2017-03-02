using System;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace APOPHIS.GroundStation.Helpers {
  public static class SerialPortService {
    private static string[] _serialPorts;

    private static ManagementEventWatcher arrival;

    private static ManagementEventWatcher removal;

    static SerialPortService() {
      _serialPorts = GetAvailableSerialPorts();
      MonitorDeviceChanges();
    }

    /// <summary>
    /// If this method isn't called, an InvalidComObjectException will be thrown (like below):
    /// System.Runtime.InteropServices.InvalidComObjectException was unhandled
    ///Message=COM object that has been separated from its underlying RCW cannot be used.
    ///Source=mscorlib
    ///StackTrace:
    ///     at System.StubHelpers.StubHelpers.StubRegisterRCW(Object pThis, IntPtr pThread)
    ///     at System.Management.IWbemServices.CancelAsyncCall_(IWbemObjectSink pSink)
    ///     at System.Management.SinkForEventQuery.Cancel()
    ///     at System.Management.ManagementEventWatcher.Stop()
    ///     at System.Management.ManagementEventWatcher.Finalize()
    ///InnerException: 
    /// </summary>
    public static void CleanUp() {
      arrival.Stop();
      removal.Stop();
    }

    public static event EventHandler<PortsChangedArgs> PortsChanged;

    private static void MonitorDeviceChanges() {
      try {
        var deviceArrivalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        var deviceRemovalQuery = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");

        arrival = new ManagementEventWatcher(deviceArrivalQuery);
        removal = new ManagementEventWatcher(deviceRemovalQuery);

        arrival.EventArrived += (sender, eventArgs) => RaisePortsChangedIfNecessary(EventType.Insertion);
        removal.EventArrived += (sender, eventArgs) => RaisePortsChangedIfNecessary(EventType.Removal);

        // Start listening for events
        arrival.Start();
        removal.Start();
      } catch (ManagementException) {

      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
    private static void RaisePortsChangedIfNecessary(EventType eventType) {
      lock (_serialPorts) {
        var availableSerialPorts = GetAvailableSerialPorts();
        if (eventType == EventType.Insertion) {
          var added = availableSerialPorts.Except(_serialPorts).ToArray();
          _serialPorts = availableSerialPorts;
          PortsChanged?.Invoke(null, new PortsChangedArgs(eventType, added));
        } else if (eventType == EventType.Removal) {
          var removed = _serialPorts.Except(availableSerialPorts).ToArray();
          _serialPorts = availableSerialPorts;
          PortsChanged?.Invoke(null, new PortsChangedArgs(eventType, removed));
        }
      }
    }

    public static string[] GetAvailableSerialPorts() {
      return SerialPort.GetPortNames().OrderBy(x => x).ToArray();
    }
  }

  public enum EventType {
    Insertion,
    Removal,
  }

  public class PortsChangedArgs : EventArgs {
    private readonly EventType _eventType;

    private readonly string[] _serialPorts;

    public PortsChangedArgs(EventType eventType, string[] serialPorts) {
      _eventType = eventType;
      _serialPorts = serialPorts;
    }

    public string[] SerialPorts {
      get {
        return _serialPorts;
      }
    }

    public EventType EventType {
      get {
        return _eventType;
      }
    }
  }
}