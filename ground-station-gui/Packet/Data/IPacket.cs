namespace APOPHIS.GroundStation.Packet.Data {
  interface IPacket {
    byte[] Bytes { get; set; }

    string CSVData { get; }

    string CSVHeader { get; }
  }
}
