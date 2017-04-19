using APOPHIS.GroundStation.Helpers;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace APOPHIS.GroundStation.Packet.Data
{
  class DataPacket : IPacket {
    //
    // Define the struct for input data from the platform.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 84)]
    public struct Packet {
      public byte magic1;
      public byte magic2;
      public byte magic3;
      public byte movement;

      public float UTC;
      public float lat;
      public float lon;
      public float alt;
      public float accelX;
      public float accelY;
      public float accelZ;
      public float gyroX;
      public float gyroY;
      public float gyroZ;
      public float magX;
      public float magY;
      public float magZ;
      public float roll;
      public float pitch;
      public float yaw;

      public byte gndmtr1;
      public byte gndmtr2;
      public byte amtr1;
      public byte amtr2;

      public byte amtr3;
      public byte amtr4;
      public byte uS1;
      public byte uS2;

      public byte uS3;
      public byte uS4;
      public byte uS5;
      public byte uS6;

      public byte payBay;
      public byte sysArmed;
      public byte padEnd1;
      public byte padEnd2;
    }

    private Packet _data;

    public byte[] Bytes {
      get {
        return _data.GetBytes();
      }
      set {
        if (value.Length != Marshal.SizeOf(_data)) throw new ArgumentException($"Array is not a valid size ({nameof(value)} ({value.Length}) != DataPacket Struct ({Marshal.SizeOf(_data)})).", nameof(value));
        _data = value.FromBytes<Packet>();
      }
    }

    public string CSVData { get { return _data.ToCSV<Packet>(fields: typeof(Packet).GetFields().Where(f => !f.Name.Contains("magic") && !f.Name.Contains("pad")).ToArray()); } }

    public string CSVHeader { get { return CSVHelpers.ToCSVHeader<Packet>(fields: typeof(Packet).GetFields().Where(f => !f.Name.Contains("magic") && !f.Name.Contains("pad")).ToArray()); } }

    public byte[] Magic { get { return new byte[] { _data.magic1, _data.magic2, _data.magic3 }; } }

    public char Movement { get { return Convert.ToChar(_data.movement); } }

    public float UTC { get { return _data.UTC; } }

    public float Latitude { get { return _data.lat; } }

    public float Longitude { get { return _data.lon; } }

    public float Altitude { get { return _data.alt; } }

    public float AccelX { get { return _data.accelX; } }

    public float AccelY { get { return _data.accelY; } }

    public float AccelZ { get { return _data.accelZ; } }

    public float GyroX { get { return _data.gyroX; } }

    public float GyroY { get { return _data.gyroY; } }

    public float GyroZ { get { return _data.gyroZ; } }

    public float MagX { get { return _data.magX * 1000000; } }

    public float MagY { get { return _data.magY * 1000000; } }

    public float MagZ { get { return _data.magZ * 1000000; } }

    public float Roll { get { return _data.roll; } }

    public float Pitch { get { return _data.pitch; } }

    public float Yaw { get { return _data.yaw; } }

    public bool GroundMeter1 { get { return Convert.ToBoolean(_data.gndmtr1); } }

    public bool GroundMeter2 { get { return Convert.ToBoolean(_data.gndmtr2); } }

    public bool AirMotor1 { get { return Convert.ToBoolean(_data.amtr1); } }

    public bool AirMotor2 { get { return Convert.ToBoolean(_data.amtr2); } }

    public bool AirMotor3 { get { return Convert.ToBoolean(_data.amtr3); } }

    public bool AirMotor4 { get { return Convert.ToBoolean(_data.amtr4); } }

    public bool uS1 { get { return Convert.ToBoolean(_data.uS1); } }

    public bool uS2 { get { return Convert.ToBoolean(_data.uS2); } }

    public bool uS3 { get { return Convert.ToBoolean(_data.uS3); } }

    public bool uS4 { get { return Convert.ToBoolean(_data.uS4); } }

    public bool uS5 { get { return Convert.ToBoolean(_data.uS5); } }

    public bool uS6 { get { return Convert.ToBoolean(_data.uS6); } }

    public bool PayloadBay { get { return Convert.ToBoolean(_data.payBay); } }

    public bool sysArmed { get { return Convert.ToBoolean(_data.sysArmed); } }

    public DataPacket(char defaultMovement = 'D') {
      _data.magic1 = 0;
      _data.magic2 = 0;
      _data.magic3 = 0;
      _data.movement = Convert.ToByte(defaultMovement);
      _data.UTC = 0;
      _data.lat = 0;
      _data.lon = 0;
      _data.alt = 0;
      _data.accelX = 0;
      _data.accelY = 0;
      _data.accelZ = 0;
      _data.gyroX = 0;
      _data.gyroY = 0;
      _data.gyroZ = 0;
      _data.magX = 0;
      _data.magY = 0;
      _data.magZ = 0;
      _data.roll = 0;
      _data.pitch = 0;
      _data.yaw = 0;
      _data.gndmtr1 = 0x0;
      _data.gndmtr2 = 0x0;
      _data.amtr1 = 0x0;
      _data.amtr2 = 0x0;
      _data.amtr3 = 0x0;
      _data.amtr4 = 0x0;
      _data.uS1 = 0x0;
      _data.uS2 = 0x0;
      _data.uS3 = 0x0;
      _data.uS4 = 0x0;
      _data.uS5 = 0x0;
      _data.uS6 = 0x0;
      _data.payBay = 0x0;
    }
  }
}