using System;
using System.Runtime.InteropServices;
using APOPHIS.GroundStation.Helpers;

namespace APOPHIS.GroundStation.Packet.Data {
  class ControlOutDataPacket : IPacket {

    //
    // Output data struct for autonomous control.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 28)]
    private struct Packet {
      public byte Magic1;
      public byte Magic2;
      public byte Magic3;
      public byte Type;          // Target or Control command? T or C?
      public float Throttle;   // Desired throttle level.
      public float Throttle2; // Desired throttle of left wheel in ground mode.
      public float Roll;         // Desired roll angle.
      public float Pitch;        // Desired pitch angle.
      public float Yaw;          // Desired yaw angle.
      public byte FlyOrDrive;     // vehicle flying or driving?
      public byte FDConfirm;      // fly or drive confirmation;
      public bool PayloadRelease;    // Release the payload command.
      public bool PRConfirm;         // Confirmation to release payload command.
    }

    private Packet _data;

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

    public float Throttle { get { return _data.Throttle; } set { _data.Throttle = value; } }

    public float Throttle2 { get { return _data.Throttle2; } set { _data.Throttle2 = value; } }

    public float Roll { get { return _data.Roll; } set { _data.Roll = value; } }

    public float Pitch { get { return _data.Pitch; } set { _data.Pitch = value; } }

    public float Yaw { get { return _data.Yaw; } set { _data.Yaw = value; } }

    public char FlyOrDrive { get { return Convert.ToChar(_data.FlyOrDrive); } set { _data.FlyOrDrive = Convert.ToByte(value); } }

    public char FDConfirm { get { return Convert.ToChar(_data.FDConfirm); } set { _data.FDConfirm = Convert.ToByte(value); } }

    public bool PayloadRelease { get { return _data.PayloadRelease; } set { _data.PayloadRelease = value; } }

    public bool PRConfirm { get { return _data.PRConfirm; } set { _data.PRConfirm = value; } }

    public ControlOutDataPacket(byte magic1 = 0xFF, byte magic2 = 0xFF, byte magic3 = 0xFF) {
      _data = new Packet();
      Magic = new byte[] { magic1, magic2, magic3 };
      Type = '0';
      Throttle = 0x00;
      Throttle2 = 0x00;
      Roll = 0x00;
      Pitch = 0x00;
      Yaw = 0x00;
      FlyOrDrive = 'D';
      FDConfirm = 'D';
      PayloadRelease = false;
      PRConfirm = false;
    }

    public ControlOutDataPacket(byte[] magic) : this() {
      Magic = magic;
    }

    public byte[] GetBytes() => _data.GetBytes();

    public void FromBytes(byte[] packetArr) {
      if (packetArr.Length != Marshal.SizeOf(_data)) throw new ArgumentException(string.Format("Array is not a valid size ({0}).", Marshal.SizeOf(_data)), nameof(packetArr));
      _data = packetArr.FromBytes<Packet>();
    }
  }
}
