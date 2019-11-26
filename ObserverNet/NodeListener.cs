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

        private void Multicast_MulticastCall(ArrayPool<byte> pool, byte[] data,int len)
        {
            Process(data,len);
            pool.Return(data);
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        public void StartRecvice()
        {
            
            multicast.Bind();
            multicast.Recvice();

        }
        
        private void Process(byte[]data,int len)
        {
            //解析数据处理
            switch(data[0])
            {
                case 1:
                    ProcessNewToic(data, len);

                    break;

                case 4:
                    ProcessNewTopicRsp(data, len);
                    break;
                case 6:
                    ProcessState(data,len);
                    break;
                case 7:
                    ProcessPubLisUpdate(data,len);
                    break;
                case 9:
                    ProcessReg(data,len);
                    break;
            }
        }

        /// <summary>
        /// 新增主题
        /// </summary>
        /// <param name="data"></param>
        private void ProcessNewToic(byte[]data,int len)
        {
            var msg = DataPack.UnPackNewTopic(data,len);
            //添加本地
            AddressInfo info = new AddressInfo();
            info.Reset(msg.Address);
            PublishList.Publish.AddNode(msg.TopicName,new AddressInfo[] { info });
            SubscribeMgr.Instance.NewTopicRec(msg.TopicName);

            multicast.SendTo(DataPack.PackNewTopicRsp(msg.TopicName, LocalNode.NodeId, LocalNode.TopicAddress));

        }

        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="data"></param>
        private void ProcessState(byte[]data,int len)
        {
            var msg = DataPack.UnPackNodeState(data,len);
            //添加本地
            NodeList.dicRefresh[msg] = DateTime.Now.Second;
        }

        /// <summary>
        /// 更新发布列表
        /// </summary>
        /// <param name="data"></param>
        private void ProcessPubLisUpdate(byte[] data,int len)
        {
            long curID = 0;
            var dic = DataPack.UnPackUpdatePublicList(data,len,out curID);
            NodeList.UpdateListCurrentID = curID;
            //添加本地
            foreach(var kv in dic)
            {
                PublishList.Publish.AddNode(kv.Key, kv.Value.ToArray());
            }
        }

        /// <summary>
        /// 解析注册
        /// </summary>
        /// <param name="data"></param>
        private void  ProcessReg(byte[] data,int len)
        {
            var msg = DataPack.UnPackReg(data,len);
            NodeList.dicRefresh[msg] = DateTime.Now.Second;
            NodeList.LstNodeInfo.Add(msg);
            int index = msg.IndexOf(",");
            string id = msg.Substring(0, index);
            NodeList.UpdateListId.Add(long.Parse(id));

        }

        private void ProcessNewTopicRsp(byte[] data, int len)
        {
            var msg = DataPack.UnPackNewTopicRsp(data, len);
            if(msg.NodeId==LocalNode.NodeId)
            {
                return;
            }
            List<string> lst = null;
            if (dicRec.TryGetValue(msg.TopicName, out lst))
            {
                lock (lst)
                {
                    if (!lst.Contains(msg.Address))
                    {
                        lst.Add(msg.Address);
                    }
                }
            }
            else
            {
                lst = new List<string>();
                dicRec[msg.TopicName] = lst;
                lst.Add(msg.Address);
            }
        }
    }
}
