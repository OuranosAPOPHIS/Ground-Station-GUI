using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APOPHISGS.Packet
{
    interface IPacket
    {
        byte[] GetBytes();

        void FromBytes(byte[] packetArr);
    }
}
