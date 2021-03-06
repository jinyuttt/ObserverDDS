﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：Subscriber
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
    public delegate void CallBackTopic(string topic, byte[] data);

    /// <summary>
    /// 订阅
    /// </summary>
    public class Subscriber
    {
        public CallBackTopic Call;

        public void Subscribe(string topic)
        {
            if (ObserverInit.isInit)
            {
                ObserverInit.Init();
            }
            SubscribeMgr.Instance.Add(this, topic);
        }

        public void UnSubscribe(string topic)
        {
            SubscribeMgr.Instance.Remove(this, topic);
        }
    }
}
