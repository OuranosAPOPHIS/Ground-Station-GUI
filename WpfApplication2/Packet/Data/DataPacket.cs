using System;
using System.Runtime.InteropServices;
using APOPHIS.GroundStation.Helpers;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;

namespace APOPHIS.GroundStation.Packet.Data {
  class DataPacket : IPacket {
    //
    // Define the struct for input data from the platform.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 84)]
    private struct Packet {
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
      public float velX;
      public float velY;
      public float velZ;
      public float posX;
      public float posY;
      public float posZ;
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
      public byte padEnd1;
      public byte padEnd2;
      public byte padEnd3;
    }

    private Packet _data;

    public byte[] Magic {
      get {
        return new byte[] { _data.magic1, _data.magic2, _data.magic3 };
      }
    }

    public char Movement { get { return Convert.ToChar(_data.movement); } }

    public float UTC { get { return _data.UTC; } }

    public float Latitude { get { return _data.lat; } }

    public float Longitude { get { return _data.lon; } }

    public float Altitude { get { return _data.alt; } }

    public float AccelX { get { return _data.accelX; } }

    public float AccelY { get { return _data.accelY; } }

    public float AccelZ { get { return _data.accelZ; } }

    public float VelX { get { return _data.velX; } }

    public float VelY { get { return _data.velY; } }

    public float VelZ { get { return _data.velZ; } }

    public float PosX { get { return _data.posX; } }

    public float PosY { get { return _data.posY; } }

    public float PosZ { get { return _data.posZ; } }

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
      _data.velX = 0;
      _data.velY = 0;
      _data.velZ = 0;
      _data.posX = 0;
      _data.posY = 0;
      _data.posZ = 0;
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

    public byte[] GetBytes() => _data.GetBytes();

    public void FromBytes(byte[] packetArr) {
      if (packetArr.Length != Marshal.SizeOf(_data)) throw new ArgumentException($"Array is not a valid size ({nameof(packetArr)} ({packetArr.Length}) != DataPacket Struct ({Marshal.SizeOf(_data)})).", nameof(packetArr));
      _data = packetArr.FromBytes<Packet>();
    }

    public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
    {
      Type t = typeof(T);
      FieldInfo[] fields = t.GetFields();

      string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

      StringBuilder csvdata = new StringBuilder();
      csvdata.AppendLine(header);

      foreach (var o in objectlist)
        csvdata.AppendLine(ToCsvFields(separator, fields, o));

      return csvdata.ToString();
    }

    public static string ToCsvFields(string separator, FieldInfo[] fields, object o)
    {
      StringBuilder linie = new StringBuilder();

      foreach (var f in fields)
      {
        if (linie.Length > 0)
          linie.Append(separator);

        var x = f.GetValue(o);

        if (x != null)
          linie.Append(x.ToString());
      }

      return linie.ToString();
    }
  }
}