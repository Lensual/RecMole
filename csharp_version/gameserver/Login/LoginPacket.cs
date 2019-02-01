using System;
using System.Collections.Generic;
using System.Text;

namespace gameserver.Login
{
    class Head : PacketBase
    {
        public Head() : base(17)
        {
            //name, offset, length, type
            base.AddMember("PkgLen", 0, 4, typeof(UInt32));
            base.AddMember("Version", 4, 1, typeof(Byte));
            base.AddMember("Command", 5, 4, typeof(UInt32));
            base.AddMember("UserID", 9, 4, typeof(UInt32));
            base.AddMember("Result", 13, 4, typeof(UInt32));
        }
    }
    class LoginPacket
    {

    }
}
