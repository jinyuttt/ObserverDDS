#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：SubscribeMgr
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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ObserverDDS
{

    /// <summary>
    /// 订阅管理
    /// </summary>
    public  class SubscribeMgr
    {
        private static readonly Lazy<SubscribeMgr> obj = new Lazy<SubscribeMgr>();
        private const int udpWait = 30;
        private const int udpTimes = 100;
       
        /// <summary>
        /// 过滤一个节点只订阅网络一次即可
        /// 订阅不成功由等待订阅主题控制
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
            List<Subscriber> lst = new List<Subscriber>();

            lst = dicSubscriber.GetOrAdd(topic, lst);

            lock (lst)
            {
                if (!lst.Contains(subscriber))
                {
                    lst.Add(subscriber);
                }
            }

            if (!dicFilter.ContainsKey(topic))
            {
                SendSubscriber(topic);
                dicFilter[topic] = null;
            }
        }

        /// <summary>
        /// 移除订阅者
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="topic"></param>
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
           
            var array =  PublishList.Publish.GetAddresses(topic);
            bool isSucess = false;
            //发送订阅信息
           if(array!=null)
            {
               //首次直接订阅
                foreach (var addr in array)
                {
                    var r = SendSubscribeTopic(topic, addr);
                    if (r)
                    {
                        isSucess = true;
                      
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
                //订阅全部没有成功;必须有一个订阅才能复制
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
        /// 直接发送订阅信息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public  bool SendSubscribeTopic(string topic,AddressInfo addr)
        {

            bool isSucess = false;
            var local = new AddressInfo();
            local.Reset(LocalNode.TopicAddress);
            byte[] tmp = DataPack.PackSubscribeMsg(topic, new AddressInfo[] { local });
            if (addr.Protol == 0)
            {
                TcpClientSocket tcp = new TcpClientSocket();
                tcp.RemoteHost = addr.Address;
                tcp.RemotePort = addr.Port;
                if (tcp.Connect())
                {
                    tcp.Send(tmp);
                    isSucess = true;
                }
                tcp.Close();
            }
            else
            {
               
                UDPSocketPack uDP = new UDPSocketPack();
                int num = udpTimes;
                while (true)
                {
                    uDP.Send(addr.Address, addr.Port, tmp);
                    byte[] buf = new byte[1024];
                    Debug.WriteLine("发送订阅信息:" + addr.ToString());
                    var tsk = Task.Factory.StartNew(() =>
                    {
                        return uDP.Recvice(buf);//只要有接收就确定收到
                    });
                    if (tsk.Wait(udpWait))
                    {
                        isSucess = true;
                        break;
                    }
                    num--;
                    if (num < 0)
                    {
                        break;
                    }
                   
                }
              

            }
            return isSucess;
        }
      
        /// <summary>
        /// 新增主题时再次订阅
        /// </summary>
        /// <param name="topic"></param>
        public void NewTopicRec(string topic,AddressInfo[] addresses)
        {
            
            if(dicSubscriber.ContainsKey(topic))
            {
                //有订阅，优先订阅等待队列
                if (NodeList.dicWaitSubscribe.ContainsKey(topic))
                {
                    //再次进入订阅,把每个发布列表订阅一遍
                    SendSubscriber(topic);
                }
                else
                {
                    //直接订阅该主题
                    foreach (var addr in addresses)
                    {
                        SendSubscribeTopic(topic, addr);
                    }
                }
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

        /// <summary>
        /// 开启线程处数据
        /// </summary>
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
                            lock (lst)
                            {
                                for (int i = 0; i < lst.Count; i++)
                                {
                                    lst[i].Call(data.TopicName, data.Data);
                                }
                            }
                        }
                    }
                }
                isRun = false;
            });
           
        }
    }
}
