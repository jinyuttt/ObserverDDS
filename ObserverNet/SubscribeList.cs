﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：SubscribeList
* 类 描 述 ：
* 所在的域 ：DESKTOP-1IBOINI
* 命名空间 ：ObserverNet
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
using System.Collections.Concurrent;

namespace ObserverNet
{

    /// <summary>
    /// 主题订阅列表（订阅方的列表）
    /// 保持订阅本节点数据的地址
    /// </summary>
  public  class SubscribeList
    {
        private static readonly Lazy<SubscribeList> instance = new Lazy<SubscribeList>();

        /// <summary>
        /// 收到的订阅信息
        /// </summary>
        private readonly ConcurrentDictionary<string, ConcurrentBag<AddressInfo>> dicList = new ConcurrentDictionary<string, ConcurrentBag<AddressInfo>>();
        public static SubscribeList Subscribe
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// 获取订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public AddressInfo[] GetAddresses(string topic)
        {
            ConcurrentBag<AddressInfo> bag = null;
            if (dicList.TryGetValue(topic,out bag))
            {
              return  bag.ToArray();
            }
            return null;
        }

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="info"></param>
        public void AddAddress(string topic,AddressInfo[] info)
        {
            if(info==null)
            {
                return;
            }
            //
            ConcurrentBag<AddressInfo> bag = null;
            if (dicList.TryGetValue(topic, out bag))
            {
                foreach(var addr in info)
                {
                    bag.Add(addr);
                }
                 
            }
            else
            {
                bag = new ConcurrentBag<AddressInfo>();
                dicList[topic] = bag;
                foreach (var addr in info)
                {
                    bag.Add(addr);
                }
            }

        }
    }
}
