#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS.Net
* 项目描述 ：
* 类 名 称 ：SessionPackage
* 类 描 述 ：
* 所在的域 ：DESKTOP-1IBOINI
* 命名空间 ：ObserverDDS.Net
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
    public class PointPackage
    {
        private readonly ConcurrentDictionary<int, UDPPackage> dic = new ConcurrentDictionary<int, UDPPackage>();
        private const int WaitTime = 5;//5秒
        public int UpdateTime { get; set; }

        public bool IsEmpty { get { return dic.IsEmpty; } }

        internal bool AddData(SubPackage package)
        {
            UpdateTime = DateTime.Now.Minute;

            UDPPackage uDP = new UDPPackage() { CreateTime = DateTime.Now.Second };
            var v = dic.GetOrAdd(package.SessionId, uDP);
            return v.AddData(package);
        }


        internal byte[] GetData(int id)
        {
            UDPPackage uDP;
            if (dic.TryRemove(id, out uDP))
            {
                List<byte> lst = new List<byte>(uDP.Packages.Length * UDPPack.PackSize);
                foreach (var p in uDP.Packages)
                {
                    if (p.Len == p.Data.Length)
                    {
                        lst.AddRange(p.Data);
                    }
                    else
                    {
                        for (int i = 0; i < p.Data.Length; i++)
                        {
                            lst.Add(p.Data[i]);
                        }
                    }
                }
                return lst.ToArray();
            }
            return null;
        }

        internal void Check()
        {
            UDPPackage package = null;
            List<int> lst = new List<int>();
            foreach (var kv in dic)
            {
                if (DateTime.Now.Second - kv.Value.CreateTime > WaitTime)
                {
                    lst.Add(kv.Key);
                }
            }
            foreach(int id in lst)
            {
                dic.TryRemove(id, out package);
            }
        }
    }
      
}
