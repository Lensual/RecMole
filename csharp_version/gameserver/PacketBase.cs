using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace gameserver
{
    class PacketBase : System.Dynamic.DynamicObject
    {
        Dictionary<string, Tuple<int, int, Type>> members =
            new Dictionary<string, Tuple<int, int, Type>>(); //name, offset, length, typecode
        public byte[] Raw;
        public PacketBase(int length)
        {
            Raw = new byte[length];
        }

        protected void AddMember(string name, int offset, int length, Type type, Object obj = null)
        {
            members.Add(name, new Tuple<int, int, Type>(offset, length, type));
            if (obj != null)
            {
                SetMember(name, obj);
            }
        }

        /// <summary>
        /// 改变对象在byte[]中的长度，会清空该对象数据
        /// </summary>
        void changeLength(string name, int newLength)
        {
            int offset = members[name].Item1;
            int oldLength = members[name].Item2;
            byte[] newRaw = new byte[Raw.Length - oldLength + newLength];
            Buffer.BlockCopy(Raw, 0, newRaw, 0, offset);    //前+自己本身
            Buffer.BlockCopy(Raw, offset + oldLength, newRaw, offset + newLength, Raw.Length - offset - oldLength); //后
            Raw = newRaw;
            //调整长度
            members[name] = new Tuple<int, int, Type>(offset, newLength, members[name].Item3);
            //调整后面成员偏移
            foreach (KeyValuePair<string, Tuple<int, int, Type>> item in members)
            {
                Tuple<int, int, Type> tuple = item.Value;
                if (tuple.Item1 > offset)
                {
                    members[item.Key] = new Tuple<int, int, Type>(item.Value.Item1 + newLength - oldLength, item.Value.Item2, item.Value.Item3);
                }
            }
        }

        void SetMember(string name, object obj)
        {
            int offset = members[name].Item1;
            int length = members[name].Item2;
            Type type = members[name].Item3;
            //处理变长对象
            if (type == typeof(string))
            {
                byte[] buffer = Encoding.UTF8.GetBytes((string)obj);
                if (length != buffer.Length)
                    changeLength(name, buffer.Length);
                Buffer.BlockCopy(buffer, 0, Raw, offset, buffer.Length);
            }
            else
            {
                byte[] buffer = ConverOrder(StructToBytes(obj));
                Buffer.BlockCopy(buffer, 0, Raw, offset, length);
                if (buffer.Length > length)
                    Console.WriteLine("object length bigger than byte array");
            }
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
                //处理变长对象
                if (type == typeof(string))
                {
                    result = Encoding.UTF8.GetString(Raw, offset, length);
                }
                else
                {
                    byte[] buffer = new byte[length];
                    Buffer.BlockCopy(Raw, offset, buffer, 0, length);
                    buffer = ConverOrder(buffer);
                    result = BytesToStruct(buffer, type);
                }
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
                SetMember(binder.Name, value);
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

        public static Object BytesToStruct(byte[] bytes, Type StructStyle)
        {
            IntPtr arrPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            return Marshal.PtrToStructure(arrPtr, StructStyle);
        }

        public static byte[] ConverOrder(byte[] data)
        {
            return data.Reverse().ToArray();
        }
    }
}
