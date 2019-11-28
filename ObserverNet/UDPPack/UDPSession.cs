using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ObserverNet
{
    /// <summary>
    /// 接收数据处理
    /// </summary>
    public class UDPSession
    {
        private readonly ConcurrentDictionary<string, PointPackage> dic = new ConcurrentDictionary<string, PointPackage>();
        private const int WaitTime = 5;//5秒
        private volatile bool IsStop = true;
        private const int TimeOut = 1;//分钟

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool AddData(string address,SubPackage package)
        {
            if (IsStop)
            {
                IsStop = false;
                Timer();
            }
            PointPackage uDP = new PointPackage() { UpdateTime = DateTime.Now.Second };
            var v = dic.GetOrAdd(address, uDP);
            return v.AddData(package);
        }

        /// <summary>
        /// 返回完成数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="sessionid"></param>
        /// <returns></returns>
        public byte[] GetData(string address,int sessionid)
        {
            PointPackage point = null;
            if(dic.TryGetValue(address,out point))
            {
                return point.GetData(sessionid);
            }
            return null;
        }


        /// <summary>
        /// 查看超时无用数据
        /// </summary>
        private void Timer()
        {
            Task.Factory.StartNew(() => {

                List<string> lst = new List<string>();
                PointPackage point;
                while(!dic.IsEmpty)
                {
                    Thread.Sleep(WaitTime * 1000);
                    foreach(var kv in dic)
                    {
                        kv.Value.Check();
                        if(kv.Value.IsEmpty&&DateTime.Now.Minute-kv.Value.UpdateTime> TimeOut)
                        {
                            lst.Add(kv.Key);
                        }
                    }
                    foreach(string addr in lst)
                    {
                        dic.TryRemove(addr, out point);
                    }
                }
                IsStop = true;
            }
            
            );
        }
    }
}
