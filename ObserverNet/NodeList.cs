#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：NodeList
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
    /// 节点信息
    /// </summary>
  public  class NodeList
    {
        /// <summary>
        /// 所有节点信息
        /// </summary>
        public static List<string> LstNodeInfo = new List<string>();

        /// <summary>
        /// 节点ID
        /// </summary>
        public static List<long> UpdateListId = new List<long>();

        /// <summary>
        /// 本节点等待订阅的主题
        /// </summary>
        public static ConcurrentDictionary<string, string> dicWaitSubscribe = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 所有节点心跳信息
        /// </summary>
        public static ConcurrentDictionary<string, int> dicRefresh = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 当前更新节点
        /// </summary>
        public static long UpdateListCurrentID = -1;

    }
}
