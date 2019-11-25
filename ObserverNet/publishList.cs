#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：publishList
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ObserverNet
{

    /// <summary>
    /// 主题发布地址列表
    /// </summary>
    public  class PublishList
    {
        private static readonly Lazy<PublishList> instance = new Lazy<PublishList>();

        private readonly ConcurrentDictionary<string, List<AddressInfo>> dicList = new ConcurrentDictionary<string, List<AddressInfo>>();
        public static PublishList  Publish
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// 获取发布地址
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public AddressInfo[] GetAddresses(string topic)
        {
            List<AddressInfo> bag = null;
            if (dicList.TryGetValue(topic, out bag))
            {
                return bag.ToArray();
            }
            return null;
        }

        /// <summary>
        /// 添加本发布地址
        /// </summary>
        /// <param name="topic"></param>
        public void AddLocal(string topic)
        {
            List<AddressInfo> bag = null;
            if (dicList.TryGetValue(topic, out bag))
            {
                bag.Add(LocalNode.InfoTcp);
            }
            else
            {
                bag = new List<AddressInfo>();
                bag.Add(LocalNode.InfoTcp);
                dicList[topic] = bag;
            }
        }

        /// <summary>
        /// 添加新的发布地址
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="addresses"></param>
        public void AddNode(string topic,AddressInfo[] addresses)
        {
            List<AddressInfo> bag = null;
            if (dicList.TryGetValue(topic, out bag))
            {
                lock (bag)
                {
                    foreach (var addr in addresses)
                    {
                        if (!bag.Contains(addr))
                        {
                            bag.Add(addr);
                        }
                    }
                }
            }
            else
            {
                bag = new List<AddressInfo>();
                dicList[topic] = bag;
                lock (bag)
                {
                    foreach (var addr in addresses)
                    {
                        if (!bag.Contains(addr))
                        {
                            bag.Add(addr);
                        }
                    }
                }
               
            }
        }

        /// <summary>
        /// 复制数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,List<AddressInfo>> CopyAddress()
        {
            Dictionary<string, List<AddressInfo>> dic = new Dictionary<string, List<AddressInfo>>();
            var lst = dicList.ToArray();
            foreach(var p in lst)
            {
                dic[p.Key] = p.Value;
            }
            return dic;
        }
    }
}
