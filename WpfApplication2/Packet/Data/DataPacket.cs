using System;
using System.Runtime.InteropServices;
using APOPHIS.GroundStation.Helpers;

namespace APOPHIS.GroundStation.Packet.Data {
  class DataPacket : IPacket {
    //
    // Define the struct for input data from the platform.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 21)]
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

      public bool gndmtr1;
      public bool gndmtr2;
      public bool amtr1;
      public bool amtr2;

      public bool amtr3;
      public bool amtr4;
      public bool uS1;
      public bool uS2;

      public bool uS3;
      public bool uS4;
      public bool uS5;
      public bool uS6;

      public bool payBay;
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

    public bool GroundMeter1 { get { return _data.gndmtr1; } }

    public bool GroundMeter2 { get { return _data.gndmtr2; } }

    public bool AirMotor1 { get { return _data.amtr1; } }

    public bool AirMotor2 { get { return _data.amtr2; } }

    public bool AirMotor3 { get { return _data.amtr3; } }

    public bool AirMotor4 { get { return _data.amtr4; } }

    public bool uS1 { get { return _data.uS1; } }

    public bool uS2 { get { return _data.uS2; } }

    public bool uS3 { get { return _data.uS3; } }

    public bool uS4 { get { return _data.uS4; } }

    public bool uS5 { get { return _data.uS5; } }

    public bool uS6 { get { return _data.uS6; } }

    public bool PayloadBay { get { return _data.payBay; } }

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
      _data.gndmtr1 = false;
      _data.gndmtr2 = false;
      _data.amtr1 = false;
      _data.amtr2 = false;
      _data.amtr3 = false;
      _data.amtr4 = false;
      _data.uS1 = false;
      _data.uS2 = false;
      _data.uS3 = false;
      _data.uS4 = false;
      _data.uS5 = false;
      _data.uS6 = false;
      _data.payBay = false;
    }

    public byte[] GetBytes() => _data.GetBytes();

    public void FromBytes(byte[] packetArr) {
      if (packetArr.Length != Marshal.SizeOf(_data)) throw new ArgumentException(string.Format("Array is not a valid size ({0}).", Marshal.SizeOf(_data)), nameof(packetArr));
      _data = packetArr.FromBytes<Packet>();
    }
  }
}