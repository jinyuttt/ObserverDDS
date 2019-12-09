#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObjectDDS
* 项目描述 ：
* 类 名 称 ：ObjectSubscriber
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




using ObserverDDS;
using System.Collections.Concurrent;
using MsgPack.Serialization;
using System.IO;

namespace ObjectNetDDS
{
    public delegate void SubscriberCallBack<T>(string topic, T data);

      
    public  class ObjectSubscriber
    {
        readonly NetSubscriber subscriber = null;
     
        private ConcurrentDictionary<string, MessagePackSerializer> dicSerializer = new ConcurrentDictionary<string, MessagePackSerializer>();
        public event SubscriberCallBack<object> CallBack;

        public ObjectSubscriber()
        {
            subscriber = new NetSubscriber();
            subscriber.CallBack += CallTopic;
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        public void Subscribe<T>(string topic)
        {
            subscriber.Subscribe(topic);
         
            dicSerializer[topic] = MsgSerializer.Serializer.GetSerializer<T>();
        }

        /// <summary>
        /// 取消主题
        /// </summary>
        /// <param name="topic"></param>
        public void UnSubscribe(string topic)
        {
            subscriber.UnSubscribe(topic);
         
        }

        private void CallTopic(string topic, byte[] data)
        {
            if (CallBack != null)
            {
                object obj = data;
                MessagePackSerializer serializer = null;
                if (dicSerializer.TryGetValue(topic, out serializer))
                {
                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        obj = serializer.Unpack(stream);

                    }
                }
                CallBack(topic, obj);
            }
        }
    }
}
