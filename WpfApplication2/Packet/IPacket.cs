namespace APOPHIS.GroundStation.Packet
{
    interface IPacket
    {
        byte[] GetBytes();

        void FromBytes(byte[] packetArr);
    }
}
