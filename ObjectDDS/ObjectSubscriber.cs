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
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace ObjectDDS
{
    public delegate void SubscriberCallBack<T>(string topic, T data);

      
    public  class ObjectSubscriber
    {
        NetSubscriber subscriber = null;
        private ConcurrentDictionary<string, Type> dic = new ConcurrentDictionary<string, Type>();
        public event SubscriberCallBack<object> CallBack;
        public ObjectSubscriber()
        {
            subscriber = new NetSubscriber();
        }
        public void Subscribe<T>(string topic)
        {
            subscriber.Subscribe(topic);
            subscriber.CallBack += CallTopic;
            dic[topic] = typeof(T);
        }

        public void UnSubscribe(string topic)
        {
            subscriber.UnSubscribe(topic);
            subscriber.CallBack -= CallTopic;
          
        }

        private void CallTopic(string topic, byte[] data)
        {
           
        }
    }
}
