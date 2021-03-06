﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：NetSubscriber
* 类 描 述 ：
* 所在的域 ：DESKTOP-1IBOINI
* 命名空间 ：ObserverDDS
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



using System;

namespace ObserverDDS
{
    public  class NetSubscriber
    {
        readonly Subscriber subscriber = null;

        /// <summary>
        ///委托回调
        /// </summary>
        public event CallBackTopic CallBack;

        static readonly Lazy<NetSubscriber> obj = new Lazy<NetSubscriber>();

        

        public NetSubscriber Instance
        {
            get { return obj.Value; }
        }
        public NetSubscriber()
        {
            subscriber = new Subscriber();
            subscriber.Call += CallTopic;
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic"></param>
        public void Subscribe(string topic)
        {
            subscriber.Subscribe(topic);
           
        }

        /// <summary>
        /// 取消主题订阅
        /// </summary>
        /// <param name="topic"></param>
        public void UnSubscribe(string topic)
        {
            subscriber.UnSubscribe(topic);
        }

        private void CallTopic(string topic,byte[]data)
        {
            if(CallBack!=null)
            {
                CallBack(topic, data);
            }
        }
    }
}
