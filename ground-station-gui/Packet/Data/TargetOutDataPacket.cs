using System;
using System.Runtime.InteropServices;
using APOPHIS.GroundStation.Helpers;
using System.Linq;

namespace APOPHIS.GroundStation.Packet.Data {
  class TargetOutDataPacket : IPacket {
    //
    // Output data struct for autonomous control.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 12)]
    public struct Packet {
      public byte Magic1;
      public byte Magic2;
      public byte Magic3;
      public byte Type;          // Target or Control command? T or C? Or A or D to indicate arm/disarm.
      public float TargetLat;    // Target Latitude
      public float TargetLong;   // Target Longitude
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

    public byte[] Magic {
      get {
        return new byte[] { _data.Magic1, _data.Magic2, _data.Magic3 };
      }
      set {
        if (value.Length != 3) throw new ArgumentOutOfRangeException("Value must have a byte array length of 3.");
        _data.Magic1 = value[0];
        _data.Magic2 = value[1];
        _data.Magic3 = value[2];
      }
    }

    public char Type { get { return Convert.ToChar(_data.Type); } set { _data.Type = Convert.ToByte(value); } }

    public float TargetLat { get { return _data.TargetLat; } set { _data.TargetLong = value; } }

    public float TargetLong { get { return _data.TargetLong; } set { _data.TargetLong = value; } }

    public TargetOutDataPacket(byte magic1 = 0xFF, byte magic2 = 0xFF, byte magic3 = 0xFF) {
      _data = new Packet();
      _data.Magic1 = magic1;
      _data.Magic2 = magic2;
      _data.Magic3 = magic3;
      Type = '0';
      TargetLat = 0;
      TargetLong = 0;
    }    
  }
}
