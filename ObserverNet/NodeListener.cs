#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：NodeListener
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
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ObserverDDS
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
                    dicRec.TryRemove(topic, out lst);//无用了
                    return true;
                }
                else if(NodeList.LstNodeInfo.Count==2&&lst.Count==2)
                {
                    //说明只有2个节点
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
        
        /// <summary>
        /// 处理接收的组播数据（寻址数据）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        internal void Process(byte[]data,int len)
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
            //添加到本地
            AddressInfo info = new AddressInfo();
            info.Reset(msg.Address);
            Debug.WriteLine("接收,ProcessNewToic:" + msg.Address);
            var lst=PublishList.Publish.AddNode(msg.TopicName,new AddressInfo[] { info });
            SubscribeMgr.Instance.NewTopicRec(msg.TopicName,lst.ToArray());//订阅
            if (!string.IsNullOrEmpty(LocalNode.TopicAddress))
            {
                //还没有完成初始化就不会返回
                byte[] tmp = DataPack.PackNewTopicRsp(msg.TopicName, LocalNode.NodeId, LocalNode.TopicAddress);
                multicast.SendTo(tmp);
                //组播信息需要桥接
                PTPMultCast.Instance.Send(tmp);
            }
           
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
            Dictionary<string, List<AddressInfo>> dicNew = new Dictionary<string, List<AddressInfo>>();
            foreach (var kv in dic)
            {
                string addr = null;
                foreach (var r in kv.Value)
                {
                    addr += (r + ",");
                }
                Debug.WriteLine("ProcessPubLisUpdate:" + addr);
                var lst = PublishList.Publish.AddNode(kv.Key, kv.Value.ToArray());
                dicNew[kv.Key] = lst;
            }

            //如果有发布地址新增主题，查看是否需要订阅
           foreach(var kv in dicNew)
            {
                SubscribeMgr.Instance.NewTopicRec(kv.Key, kv.Value.ToArray());
            }
           
           if(PublishList.Publish.IsUpdate)
            {
                //如果发布列表有修改不一致，立即触发全网节点更新，但不影响正常的更新顺序
                var bytes = DataPack.PackTriggerUpdatePublicList(LocalNode.NodeId, PublishList.Publish.CopyAddress());
                multicast.SendTo(bytes);
                //组播信息需要桥接
                PTPMultCast.Instance.Send(bytes);
                PublishList.Publish.IsUpdate = false;
            }

        }


        /// <summary>
        /// 触发更新发布列表
        /// </summary>
        /// <param name="data"></param>
        private void ProcessTriggerPubLisUpdate(byte[] data, int len)
        {

            long nodeid = -1;
            var dic = DataPack.UnPackTriggerUpdatePublicList(data, len,out nodeid);
            if(nodeid==LocalNode.NodeId)
            {
                //本节点不再更新
                return;
            }
      
            //添加本地
        
            Dictionary<string, List<AddressInfo>> dicNew = new Dictionary<string, List<AddressInfo>>();
            foreach (var kv in dic)
            {
                string addr = null;
               foreach(var r in kv.Value)
                {
                    addr += (r + ",");
                }
                Debug.WriteLine("ProcessTriggerPubLisUpdate:" + addr);
                var lst = PublishList.Publish.AddNode(kv.Key, kv.Value.ToArray());
                dicNew[kv.Key] = lst;


            }
            foreach (var topic in dicNew)
            {
                
                //如果有发布地址新增主题，查看是否需要订阅
                foreach (var kv in dicNew)
                {
                    SubscribeMgr.Instance.NewTopicRec(kv.Key, kv.Value.ToArray());
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
            var bytes = DataPack.PackTriggerUpdatePublicList(LocalNode.NodeId, PublishList.Publish.CopyAddress());
            multicast.SendTo(bytes);

            //立即触发全节点更新
            NodeTimer.Instance.UpdateList();

            //组播信息需要桥接
            PTPMultCast.Instance.Send(bytes);
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
