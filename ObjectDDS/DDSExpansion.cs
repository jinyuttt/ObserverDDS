#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObjectDDS
* 项目描述 ：
* 类 名 称 ：DDSExpansion
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
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectDDS
{
   
    public static class DDSExpansion
    {
        public static  void Publish<T>(this NetPublisher publisher,string topic, T obj)
        {
           var data= MsgSerializer.Serializer.Serialize(obj);
            publisher.Publish(topic, data);
        }

        public static void Subscribe<T>(this NetSubscriber  subscriber, string topic)
        {
            subscriber.Subscribe(topic);
            subscriber.CallBack += Subscriber_CallBack;
        }

        private static void Subscriber_CallBack(string topic, byte[] data)
        {
           
        }
    }
}
