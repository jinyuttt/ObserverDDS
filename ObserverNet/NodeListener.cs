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
        readonly MulticastSocket multicast = new MulticastSocket();

        
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
                case 10:
                    ProcessTriggerPubLisUpdate(data, len);
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
            NodeList.UpdateListCurrentID = curID;//更新轮训节点
            //添加本地
            List<string> lstNew = new List<string>();
            foreach(var kv in dic)
            {
               bool isAdd= PublishList.Publish.AddNode(kv.Key, kv.Value.ToArray());
                if (isAdd)
                {
                    lstNew.Add(kv.Key);
                }
            }
            foreach(var topic in lstNew)
            {
                //如果有发布地址新增主题，查看是否正在等待订阅
                if (NodeList.dicWaitSubscribe.ContainsKey(topic))
                {
                    //再次订阅
                    SubscribeMgr.Instance.NewTopicRec(topic);
                }
            }
           if(PublishList.Publish.IsUpdate)
            {
                //如果发布列表有修改不一致，立即触发全网节点更新，但不影响正常的更新顺序
                var bytes = DataPack.PackTriggerUpdatePublicList(LocalNode.NodeId, PublishList.Publish.CopyAddress());
                multicast.SendTo(bytes);
                PublishList.Publish.IsUpdate = false;
            }

        }


        /// <summary>
        /// 触发更新发布列表
        /// </summary>
        /// <param name="data"></param>
        private void ProcessTriggerPubLisUpdate(byte[] data, int len)
        {
         
            var dic = DataPack.UnPackTriggerUpdatePublicList(data, len);
      
            //添加本地
            List<string> lstNew = new List<string>();
            foreach (var kv in dic)
            {
                bool isAdd = PublishList.Publish.AddNode(kv.Key, kv.Value.ToArray());
                if (isAdd)
                {
                    lstNew.Add(kv.Key);
                }
            }
            foreach (var topic in lstNew)
            {
                //如果有发布地址新增主题，查看是否正在等待订阅
                if (NodeList.dicWaitSubscribe.ContainsKey(topic))
                {
                    //再次订阅
                    SubscribeMgr.Instance.NewTopicRec(topic);
                }
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
            //如果有节点注册，则立即触发一次发布列表刷新
            NodeTimer.Instance.UpdateList();
        }

        /// <summary>
        /// 处理新增主题
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
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
