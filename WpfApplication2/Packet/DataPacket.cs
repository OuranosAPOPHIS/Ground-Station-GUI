using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using APOPHISGS.Helpers;

namespace APOPHISGS.Packet
{
    class DataPacket : IPacket
    {
        //
        // Define the struct for input data from the platform.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 28)]
        private struct Packet
        {
            public byte magic1;
            public byte magic2;
            public byte magic3;
            public char movement;
            public float UTC;
            public float lat, lon, alt;
            public float accelX, accelY, accelZ;
            public float velX, velY, velZ;
            public float posX, posY, posZ;
            public float roll, pitch, yaw;
            public bool gndmtr1, gndmtr2;
            public bool amtr1, amtr2, amtr3, amtr4;
            public bool uS1, uS2, uS3, uS4, uS5, uS6;
            public bool payBay;
        }

        private Packet data;

        public byte[] Magic
        {
            get
            {
                return new byte[] { data.magic1, data.magic2, data.magic3 };
            }
        }

        public char Movement { get { return data.movement; } }

        public float UTC { get { return data.UTC; } }

        public float Latitude { get { return data.lat; } }

        public float Longitude { get { return data.lon; } }

        public float Altitude { get { return data.alt; } }

        public float AccelX { get { return data.accelX; } }

        public float AccelY { get { return data.accelY; } }

        public float AccelZ { get { return data.accelZ; } }

        public float VelX { get { return data.velX; } }

        public float VelY { get { return data.velY; } }

        public float VelZ { get { return data.velZ; } }

        public float PosX { get { return data.posX; } }

        public float PosY { get { return data.posY; } }

        public float PosZ { get { return data.posZ; } }

        public float Roll { get { return data.roll; } }

        public float Pitch { get { return data.pitch; } }

        public float Yaw { get { return data.yaw; } }

        public bool GroundMeter1 { get { return data.gndmtr1; } }

        public bool GroundMeter2 { get { return data.gndmtr2; } }

        public bool AirMotor1 { get { return data.amtr1; } }

        public bool AirMotor2 { get { return data.amtr2; } }

        public bool AirMotor3 { get { return data.amtr3; } }

        public bool AirMotor4 { get { return data.amtr4; } }

        public bool uS1 { get { return data.uS1; } }

        public bool uS2 { get { return data.uS2; } }

        public bool uS3 { get { return data.uS3; } }

        public bool uS4 { get { return data.uS4; } }

        public bool uS5 { get { return data.uS5; } }

        public bool uS6 { get { return data.uS6; } }

        public bool PayloadBay { get { return data.payBay; } }

        public DataPacket(char defaultMovement = 'D')
        {
            data.magic1 = 0;
            data.magic2 = 0;
            data.magic3 = 0;
            data.movement = defaultMovement;
            data.UTC = 0;
            data.lat = 0;
            data.lon = 0;
            data.alt = 0;
            data.accelX = 0;
            data.accelY = 0;
            data.accelZ = 0;
            data.velX = 0;
            data.velY = 0;
            data.velZ = 0;
            data.posX = 0;
            data.posY = 0;
            data.posZ = 0;
            data.roll = 0;
            data.pitch = 0;
            data.yaw = 0;
            data.gndmtr1 = false;
            data.gndmtr2 = false;
            data.amtr1 = false;
            data.amtr2 = false;
            data.amtr3 = false;
            data.amtr4 = false;
            data.uS1 = false;
            data.uS2 = false;
            data.uS3 = false;
            data.uS4 = false;
            data.uS5 = false;
            data.uS6 = false;
            data.payBay = false;
        }

        public byte[] GetBytes() => data.GetBytes();

        public void FromBytes(byte[] packetArr)
        {
            if (packetArr.Length != Marshal.SizeOf(data)) throw new ArgumentException(string.Format("Array is not a valid size ({0}).", Marshal.SizeOf(data)), nameof(packetArr));
            data = packetArr.FromBytes<Packet>();
        }
    }
}