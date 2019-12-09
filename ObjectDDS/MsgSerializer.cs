#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObjectDDS
* 项目描述 ：
* 类 名 称 ：MsgSer
* 类 描 述 ：
* 命名空间 ：ObjectDDS
* 机器名称 ：DESKTOP-1IBOINI
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu  
* 创建时间 ：2019
* 更新时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion




using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObjectNetDDS
{
   public class MsgSerializer
    {
        private static Lazy<MsgSerializer> serializer = new Lazy<MsgSerializer>();
        public static MsgSerializer Serializer
        {
            get { return serializer.Value; }
        }
        readonly SerializationContext context = new SerializationContext(MsgPack.PackerCompatibilityOptions.None);
        public byte[] Serialize<T>(T obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                var ser = MessagePackSerializer.Get<T>(context);
               
                ser.Pack(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }
        public T DeSerialize<T>(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                var ser = MessagePackSerializer.Get<T>(context);

                var obj = ser.Unpack(memoryStream);
                return obj;
            }
        }

        public MessagePackSerializer GetSerializer<T>()
        {
            var serializer = MessagePackSerializer.Get<T>();

            return serializer;
        }
    }
}
