namespace APOPHIS.GroundStation.Packet.Data {
  interface IPacket {
    byte[] GetBytes();

    void FromBytes(byte[] packetArr);
  }
}
