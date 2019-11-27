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

        private readonly ConcurrentQueue<TopicData> queue = new ConcurrentQueue<TopicData>();

        private bool isRun = false;
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
                lock (lst)
                {
                    if (!lst.Contains(subscriber))
                    {
                        lst.Add(subscriber);
                    }
                }
            }
            else
            {
                lst = new List<Subscriber>();
                lock (lst)
                {
                    dicSubscriber[topic] = lst;
                    lst.Add(subscriber);
                }
            }
            SendSubscriber(topic);
        }

        public void Remove(Subscriber subscriber, string topic)
        {
            List<Subscriber> lst = null;

            if (dicSubscriber.TryGetValue(topic, out lst))
            {
                lock (lst)
                {
                    lst.Remove(subscriber);
                }
            }
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
            bool isSucess = false;
            //发送订阅信息
           if(array!=null)
            {
                var local = new AddressInfo();
                local.Reset(LocalNode.TopicAddress);
                byte[] tmp = DataPack.PackSubscribeMsg(topic,new AddressInfo[] {local  });
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
                            isSucess = true;
                        }
                        tcp.Close();
                    }
                    else
                    {
                        // UDPSocket uDP = new UDPSocket();
                        UDPSocketPack uDP = new UDPSocketPack();
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
                                isSucess = true;
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
           if(!isSucess)
            {
                //订阅全部没有成功
                NodeList.dicWaitSubscribe[topic] = null;
                string t;
                dicFilter.TryRemove(topic,out t);
            }
           else
            {
                string t;
                //订阅有一个成功
                NodeList.dicWaitSubscribe.TryRemove(topic, out t);
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
    
        /// <summary>
        /// 返回数据
        /// </summary>
        /// <param name="data"></param>
        public void AddData(TopicData data)
        {
            queue.Enqueue(data);
            if(isRun)
            {
                return;
            }
            ThreadQueue();
        }

        private void ThreadQueue()
        {
            isRun = true;
            Task.Factory.StartNew(() =>
            {
                TopicData data = null;
                List<Subscriber> lst = null;
                while (!queue.IsEmpty)
                {
                    if (queue.TryDequeue(out data))
                    {
                        if (dicSubscriber.TryGetValue(data.TopicName, out lst))
                        {
                            for (int i = 0; i < lst.Count; i++)
                            {
                                lst[i].Call(data.TopicName, data.Data);
                            }
                        }
                    }
                }
                isRun = false;
            });
           
        }
    }
}
