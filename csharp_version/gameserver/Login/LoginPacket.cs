using System;
using System.Collections.Generic;
using System.Text;

namespace gameserver.Login
{
    class Head : PacketBase
    {
        public Head(UInt32 cmdId, UInt32 userId, UInt32 errorId, UInt32 bodylen) : base(17)
        {
            //name, offset, length, type
            base.AddMember("PkgLen", 0, 4, typeof(UInt32), base.Raw.Length + bodylen);
            base.AddMember("Version", 4, 1, typeof(Byte), 0);
            base.AddMember("Command", 5, 4, typeof(UInt32), cmdId);
            base.AddMember("UserID", 9, 4, typeof(UInt32), userId);
            base.AddMember("Result", 13, 4, typeof(UInt32), errorId);
        }
    }
    class LoginPacket
    {

    }
}
