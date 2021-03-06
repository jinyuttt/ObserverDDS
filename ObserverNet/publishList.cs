﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：publishList
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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ObserverDDS
{

    /// <summary>
    /// 主题发布地址列表
    /// </summary>
    public  class PublishList
    {
        private static readonly Lazy<PublishList> instance = new Lazy<PublishList>();

        private readonly ConcurrentDictionary<string, List<AddressInfo>> dicList = new ConcurrentDictionary<string, List<AddressInfo>>();

        /// <summary>
        /// 发布列表有修改
        /// </summary>
        public bool IsUpdate { get; set; }

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
            List<AddressInfo> bag = new List<AddressInfo>();
            var lst = dicList.GetOrAdd(topic, bag);
           
                lock (lst)
                {
                    var addr = new AddressInfo();
                    addr.Reset(LocalNode.TopicAddress);
                    if (!lst.Contains(addr))
                    {
                    lst.Add(addr);
                    }
                }
           
        }

        
        /// <summary>
        /// 新增发布地址
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="addresses">地址</param>
        /// <returns>新增的节点发布</returns>
        public List<AddressInfo> AddNode(string topic,AddressInfo[] addresses)
        {
            List<AddressInfo> lstNew = new List<AddressInfo>();
            List<AddressInfo> bag = new List<AddressInfo>();
            bag = dicList.GetOrAdd(topic, bag);
            lock (bag)
            {
                foreach (var addr in addresses)

                {
                    if (!bag.Contains(addr))
                    {
                        bag.Add(addr);
                        lstNew.Add(addr);
                        IsUpdate = true;
                       
                    }
                }
            }
            return lstNew;
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
