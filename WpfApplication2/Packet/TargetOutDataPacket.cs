using System;
using System.Runtime.InteropServices;
using APOPHIS.GroundStation.Helpers;

namespace APOPHIS.GroundStation.Packet
{
    class TargetOutDataPacket : IPacket
    {
        //
        // Output data struct for autonomous control.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 12)]
        private struct Packet
        {
            public byte Magic1;
            public byte Magic2;
            public byte Magic3;
            public byte Type;          // Target or Control command? T or C?
            public float TargetLat;    // Target Latitude
            public float TargetLong;   // Target Longitude
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

        public float TargetLat { get { return data.TargetLat; } set { data.TargetLong = value; } }

        public float TargetLong { get { return data.TargetLong; } set { data.TargetLong = value; } }

        public TargetOutDataPacket(byte magic1 = 0xFF, byte magic2 = 0xFF, byte magic3 = 0xFF)
        {
            data = new Packet();
            data.Magic1 = magic1;
            data.Magic2 = magic2;
            data.Magic3 = magic3;
            Type = '0';
            TargetLat = 0;
            TargetLong = 0;
        }

        public byte[] GetBytes() => data.GetBytes();

        public void FromBytes(byte[] packetArr)
        {
            if (packetArr.Length != Marshal.SizeOf(data)) throw new ArgumentException(string.Format("Array is not a valid size ({0}).", Marshal.SizeOf(data)), nameof(packetArr));
            data = packetArr.FromBytes<Packet>();
        }
    }
}
