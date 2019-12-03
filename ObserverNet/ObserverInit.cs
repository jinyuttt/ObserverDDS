#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：ObserverInit
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
using System.Diagnostics;
using System.Text;

namespace ObserverDDS
{

   /// <summary>
   /// 节点初始化
   /// </summary>
  public  class ObserverInit
    {
        static object lock_obj = new object();
      public  static bool isInit = true;
       public static void Init()
        {
           
            lock (lock_obj)
            {
                if(!isInit)
                {
                    return;
                }
                //开启网络
                NodeListener.Instance.StartRecvice();
                SubscribeMessage.Instance.Init();
                NodeTimer.Instance.Start();
               
                //初始化注册
                Random random = new Random(DateTime.Now.Millisecond);
                LocalNode.NodeId = random.Next();
                LocalNode.InfoTcp = new AddressInfo();
                LocalNode.InfoTcp.Reset("0_" + SubscribeMessage.Instance.TcpAddress);
                LocalNode.InfoUdp = new AddressInfo();
                LocalNode.InfoUdp.Reset("1_" + SubscribeMessage.Instance.UdpAddress);
                LocalNode.TopicAddress = LocalNode.InfoUdp.ToString();
                NodeTimer.Instance.SendReg();

                Console.WriteLine(LocalNode.NodeId + ";" + LocalNode.InfoUdp + ";" + LocalNode.InfoTcp);
                isInit = false;
            }
         
        }
    }
}
