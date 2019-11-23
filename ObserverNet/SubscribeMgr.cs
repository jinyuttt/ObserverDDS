#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：SubscribeMgr
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
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ObserverNet
{
    public  class SubscribeMgr
    {
        private static readonly Lazy<SubscribeMgr> obj = new Lazy<SubscribeMgr>();
        private const int udpWait = 30;
        private const int udpTimes = 100;

        /// <summary>
        /// 过滤一个节点只订阅网络一次即可
        /// </summary>
        private readonly ConcurrentDictionary<string, string> dicFilter = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, List<Subscriber>> dicSubscriber = new ConcurrentDictionary<string, List<Subscriber>>();


        public static SubscribeMgr Instance
        {
            get { return obj.Value; }
        }

        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="topic"></param>
        public void Add(Subscriber subscriber, string topic)
        {
            List<Subscriber> lst = null;

            if (dicSubscriber.TryGetValue(topic, out lst))
            {
                if (!lst.Contains(subscriber))
                {
                    lst.Add(subscriber);
                }
            }
            else
            {
                lst = new List<Subscriber>();
                dicSubscriber[topic] = lst;
                lst.Add(subscriber);
            }
            SendSubscriber(topic);
        }


        /// <summary>
        /// 发送订阅信息
        /// </summary>
        /// <param name="topic"></param>
        private void SendSubscriber(string topic)
        {
            if(dicFilter.ContainsKey(topic))
            {
                return;
            }
          dicFilter[topic] = null;
          var array=  PublishList.Publish.GetAddresses(topic);

            //发送订阅信息
           if(array!=null)
            {
                byte[] tmp = DataPack.PackSubscribeMsg(topic,new AddressInfo[] { LocalNode.InfoTcp, LocalNode.InfoUdp });
                foreach (var addr in array)
                {
                    if(addr.Protol==0)
                    {
                        TcpClientSocket tcp = new TcpClientSocket();
                        tcp.RemoteHost = addr.Address;
                        tcp.RemotePort = addr.Port;
                        if(tcp.Connect())
                        {
                            tcp.Send(tmp);
                        }
                        tcp.Close();
                    }
                    else
                    {
                        UDPSocket uDP = new UDPSocket();
                        int num = udpTimes;
                        while (true)
                        {
                            uDP.Send(addr.Address, addr.Port, tmp);
                            byte[] buf = new byte[3];

                            var tsk = Task.Factory.StartNew(() =>
                              {
                                  return uDP.Recvice(buf);//只要有接收就确定收到
                              });
                            if(tsk.Wait(udpWait))
                            {
                                break;
                            }
                            num--;
                            if(num<0)
                            {
                                break;
                            }
                        }
                        
                    }
                }
            }
            else
            {
                //没有发布地址，进入等待订阅列表
                NodeList.dicWaitSubscribe[topic] = null;
                //
            }
        }

        /// <summary>
        /// 新增主题时再次订阅
        /// </summary>
        /// <param name="topic"></param>
        public void NewTopicRec(string topic)
        {
            if (NodeList.dicWaitSubscribe.ContainsKey(topic))
            {
                //再次进入订阅
                SendSubscriber(topic);
            }
        }
    }
}
