using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace gameserver
{
    class PacketBase : System.Dynamic.DynamicObject
    {
        Dictionary<string, Tuple<int, int, Type>> members =
            new Dictionary<string, Tuple<int, int, Type>>(); //name, offset, length, typecode
        public byte[] Raw;
        public PacketBase(uint length)
        {
            Raw = new byte[length];
        }

        protected void AddMember(string name, int offset, int length, Type type, Object obj = null)
        {
            members.Add(name, new Tuple<int, int, Type>(offset, length, type));
            if (obj != null)
            {
                setObj(obj, offset, length);
            }
        }

        void setObj(object obj,int offset,int length)
        {
            byte[] buffer = StructToBytes(obj);
            Buffer.BlockCopy(buffer, 0, Raw, offset, length);
            if (buffer.Length > length)
                Console.WriteLine("object length bigger than byte array");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            Tuple<int, int, Type> tuple;
            if (members.TryGetValue(name, out tuple))
            {
                int offset = members[binder.Name].Item1;
                int length = members[binder.Name].Item2;
                Type type = members[binder.Name].Item3;
                result = BytesToStruct(Raw, type, offset);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string name = binder.Name;
            if (members.ContainsKey(name))
            {
                int offset = members[binder.Name].Item1;
                int length = members[binder.Name].Item2;
                setObj(value, offset, length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static byte[] StructToBytes(Object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] bytes = new byte[size];
            IntPtr arrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            Marshal.StructureToPtr(obj, arrPtr, true);
            return bytes;
        }

        public static Object BytesToStruct(byte[] bytes, Type StructStyle, int offset = 0)
        {
            IntPtr arrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, offset);
            return Marshal.PtrToStructure(arrPtr, StructStyle);
        }
    }
}
