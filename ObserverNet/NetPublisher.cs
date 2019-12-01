#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：NetPublisher
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
using System.Collections.Generic;
using System.Text;

namespace ObserverDDS
{
  public  class NetPublisher
    {
        private readonly Publisher publisher = null;
        static readonly Lazy<NetPublisher> obj = new Lazy<NetPublisher>();

        public NetPublisher Instance
        {
            get { return obj.Value; }
        }
        public NetPublisher()
        {
            publisher = new Publisher();
        }

        public   void Publish(string topic,byte[]data)
        {
            publisher.Publish(topic, data);
        }
    }
}
