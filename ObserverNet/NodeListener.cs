#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：NodeListener
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
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ObserverNet
{

    /// <summary>
    /// 节点组播信息
    /// </summary>
    public  class NodeListener
    {
        private readonly ArrayPool<byte> poolData = ArrayPool<byte>.Create(1024 * 1024, 10);

        private static readonly Lazy<NodeListener> obj = new Lazy<NodeListener>();

        private ConcurrentDictionary<string, List<string>> dicRec = new ConcurrentDictionary<string, List<string>>();
        private ConcurrentDictionary<string,string> dicComplete = new ConcurrentDictionary<string,string>();

        MulticastSocket multicast = new MulticastSocket();

        
        public static NodeListener Instance
        {
            get { return obj.Value; }
        }

        /// <summary>
        /// 节点接收到信息
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public bool IsComplete(string topic)
        {

            List<string> lst;
            if (dicRec.TryGetValue(topic, out lst))
            {

                if (lst.Count > NodeList.LstNodeInfo.Count / 2)
                {
                    return true;
                }


            }
            return false;

        }
        public NodeListener()
        {
            multicast.MulticastCall += Multicast_MulticastCall;
        }

        private void Multicast_MulticastCall(ArrayPool<byte> pool, byte[] data)
        {
            Process(data);
            pool.Return(data);
        }

        private void StartRecvice()
        {
            
            multicast.Bind();
            multicast.Recvice();

        }
        
        private void Process(byte[]data)
        {
            //解析数据处理
            switch(data[0])
            {
                case 1:
                    ProcessNewToic(data);

                    break;
                case 6:
                    ProcessState(data);
                    break;
            }
        }

        private void ProcessNewToic(byte[]data)
        {
            var msg = DataPack.UnPackNewTopic(data);
            //添加本地
            AddressInfo info = new AddressInfo();
            info.Reset(msg.Address);
            PublishList.Publish.AddNode(msg.TopicName,new AddressInfo[] { info });
            SubscribeMgr.Instance.NewTopicRec(msg.TopicName);

        }

        private void ProcessState(byte[]data)
        {
            var msg = DataPack.UnPackNodeState(data);
            //添加本地
            NodeList.dicRefresh[msg] = DateTime.Now.Second;
        }
    }
}
