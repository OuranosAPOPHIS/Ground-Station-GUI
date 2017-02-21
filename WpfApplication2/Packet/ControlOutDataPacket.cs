using System;
using System.Runtime.InteropServices;
using APOPHIS.GroundStation.Helpers;

namespace APOPHIS.GroundStation.Packet
{
    class ControlOutDataPacket : IPacket
    {

        //
        // Output data struct for autonomous control.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 28)]
        private struct Packet
        {
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

        private Packet data;

        public byte[] Magic
        {
            get
            {
                return new byte[] { data.Magic1, data.Magic2, data.Magic3 };
            }
            set
            {
                if (value.Length != 3) throw new ArgumentOutOfRangeException("Value must have a byte array length of 3.");
                data.Magic1 = value[0];
                data.Magic2 = value[1];
                data.Magic3 = value[2];
            }
        }

        public char Type { get { return Convert.ToChar(data.Type); } set { data.Type = Convert.ToByte(value); } }

        public float Throttle { get { return data.Throttle; } set { data.Throttle = value; } }

        public float Throttle2 { get { return data.Throttle2; } set { data.Throttle2 = value; } }

        public float Roll { get { return data.Roll; } set { data.Roll = value; } }

        public float Pitch { get { return data.Pitch; } set { data.Pitch = value; } }

        public float Yaw { get { return data.Yaw; } set { data.Yaw = value; } }

        public char FlyOrDrive { get { return Convert.ToChar(data.FlyOrDrive); } set { data.FlyOrDrive = Convert.ToByte(value); } }

        public char FDConfirm { get { return Convert.ToChar(data.FDConfirm); } set { data.FDConfirm = Convert.ToByte(value); } }

        public bool PayloadRelease { get { return data.PayloadRelease; } set { data.PayloadRelease = value; } }

        public bool PRConfirm { get { return data.PRConfirm; } set { data.PRConfirm = value; } }

        public ControlOutDataPacket(byte magic1 = 0xFF, byte magic2 = 0xFF, byte magic3 = 0xFF)
        {
            data = new Packet();
            data.Magic1 = magic1;
            data.Magic2 = magic2;
            data.Magic3 = magic3;
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

        public byte[] GetBytes() => data.GetBytes();

        public void FromBytes(byte[] packetArr)
        {
            if (packetArr.Length != Marshal.SizeOf(data)) throw new ArgumentException(string.Format("Array is not a valid size ({0}).", Marshal.SizeOf(data)), nameof(packetArr));
            data = packetArr.FromBytes<Packet>();
        }
    }
}
