#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：UDPPackProcess
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObserverDDS
{

    /// <summary>
    /// 管理公共资源
    /// </summary>
  public  class UDPPackProcess
    {
        private static Lazy<UDPPackProcess> Obj = new Lazy<UDPPackProcess>();

        private  int Sessionid = int.MinValue;
        /// <summary>
        /// 筛选重复数据
        /// </summary>
      public  readonly ConcurrentDictionary<string, int> dicFilter = new ConcurrentDictionary<string, int>();

        public static UDPPackProcess Instance
        {
            get { return Obj.Value; }
        }

        public UDPPackProcess()
        {
            Random random = new Random(DateTime.Now.Millisecond);
            Sessionid = random.Next();
            Timer();
        }

        private void Timer()
        {

               Thread timer = new Thread(new ParameterizedThreadStart((obj) =>
              {
                   while(true)
                  {
                      Thread.Sleep(20000);
                      int time = 0;
                      List<string> lst = new List<string>();
                      foreach (var kv in dicFilter)
                      {
                          if (DateTime.Now.Second - kv.Value > 15)
                          {
                              //传输都是5秒以内自动丢失，15秒足够
                              lst.Add(kv.Key);
                          }
                      }
                      foreach(var key in lst)
                      {
                          dicFilter.TryRemove(key, out time);
                      }
                  }
              }));
            timer.IsBackground = true;
            timer.Name = "UDPFilter";
            timer.Start();
           
        }
        /// <summary>
        /// 唯一ID
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return Interlocked.Increment(ref Sessionid);
        }
    }
}
