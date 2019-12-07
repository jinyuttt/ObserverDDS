#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：NewTopicPub
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
using System.Text;
using System.Threading;

namespace ObserverDDS
{


    /// <summary>
    /// 新增主题
    /// </summary>
    public  class NewTopicPub
    {
        private static readonly Lazy<NewTopicPub> instance = new Lazy<NewTopicPub>();

        private const int WaitNum = 20;

        public static NewTopicPub  Pub
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// 广播新主题
        /// </summary>
        /// <param name="topic"></param>
        public void SendNewTopic(string topic)
        {
            MulticastSocket multicast = new MulticastSocket();
            int num = WaitNum;
            var bytes = DataPack.PackNewTopic(topic, LocalNode.NodeId, LocalNode.TopicAddress);
            while (!NodeListener.Instance.IsComplete(topic))
            {
                num--;
                multicast.SendTo(bytes);
                Thread.Sleep(50);
               if(num<0)
                {
                    break;
                }
            }
            //组播信息需要桥接
            PTPMultCast.Instance.Send(bytes);
            multicast.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="nodeAddr"></param>
        /// <returns></returns>
        public bool CopyAddress(string topic,AddressInfo nodeAddr)
        {
            TcpClientSocket tcpClient = new TcpClientSocket();
            tcpClient.RemoteHost = nodeAddr.Address;
            tcpClient.RemotePort = nodeAddr.Port;
            if(tcpClient.Connect())
            {
                tcpClient.Send(Encoding.Default.GetBytes(topic));
            }
            return true;
        }
    }
}
