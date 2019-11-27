#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：NodeTimer
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
using System.Threading;
using System.Threading.Tasks;

namespace ObserverNet
{

    /// <summary>
    /// 节点定时任务
    /// </summary>
    public class NodeTimer
    {
        private static readonly Lazy<NodeTimer> obj = new Lazy<NodeTimer>();

        private readonly MulticastSocket Multicast = new MulticastSocket();

        private const int WaitTime = 10 * 1000;

        private readonly byte[] tectBytes = new byte[] { 8, 1, 1, 1 };

        private const int WaitTectNum = 100;

        private int CountUpdateNum = 0;

        private long CurrentId = -1;

        public static NodeTimer Instance
        {
            get { return obj.Value; }
        }

        /// <summary>
        /// 启动定时处理节点
        /// </summary>
        public void Start()
        {
          
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(WaitTime);
                UpdateList();
                Heart();
                Start();
            });
          

        }
        

        /// <summary>
        /// 心跳包
        /// </summary>
        private void Heart()
        {
            Task.Factory.StartNew(() =>
            {
               
                var msg = DataPack.PackNodeState(LocalNode.NodeId, LocalNode.TopicAddress);
                Multicast.SendTo(msg);
                //
                if (NodeList.LstNodeInfo.Count == 0)
                {
                    int num = NodeList.dicRefresh.Count;
                    string[] keys = new string[num];
                    NodeList.dicRefresh.Keys.CopyTo(keys, 0);
                    NodeList.LstNodeInfo.AddRange(keys);
                    //取出ID
                    foreach (var ky in keys)
                    {
                        int index = ky.IndexOf(",");
                        string id = ky.Substring(0, index);
                        NodeList.UpdateListId.Add(long.Parse(id));
                    }
                }
                else
                {
                    var array = NodeList.LstNodeInfo.ToArray();
                    foreach (var node in array)
                    {
                        int Sp = 0;
                        NodeList.dicRefresh.TryGetValue(node, out Sp);
                        if (Math.Abs(DateTime.Now.Second - Sp) / WaitTime > 2)
                        {
                            //启动侦测
                            int index = node.IndexOf(",");
                            string addr = node.Substring(index);
                            string[] address = addr.Split('_');
                            if (address[0] == "0")
                            {
                                TcpClientSocket tcpClient = new TcpClientSocket();
                                tcpClient.RemoteHost = address[0];
                                tcpClient.RemotePort = int.Parse(address[1]);
                                if (tcpClient.Connect())
                                {
                                    //刷新
                                    NodeList.dicRefresh[node] = DateTime.Now.Second;
                                }
                                else
                                {
                                    //移除节点信息
                                    string id = node.Substring(0, index);
                                    NodeList.LstNodeInfo.Remove(node);
                                    NodeList.UpdateListId.Remove(long.Parse(id));
                                    AddressInfo info = new AddressInfo();
                                    info.Reset(addr);
                                    SubscribeList.Subscribe.Remove(info);
                                }
                               
                              
                            }
                            else
                            {
                                UDPSocket uDP = new UDPSocket();
                                byte[] buf = new byte[1024];
                                int num = WaitTectNum;
                                while (num > 0)
                                {
                                    uDP.Send(address[0], int.Parse(address[1]), tectBytes);
                                    var tsk = Task.Factory.StartNew(() =>
                                    {
                                        return uDP.Recvice(buf);

                                    });
                                    if (tsk.Wait(50))
                                    {
                                        NodeList.dicRefresh[node] = DateTime.Now.Second;
                                        break;
                                    }
                                    num--;
                                }
                                if (num <= 0)
                                {
                                    //移除节点信息
                                    NodeList.LstNodeInfo.Remove(node);
                                    AddressInfo info = new AddressInfo();
                                    info.Reset(addr);
                                    SubscribeList.Subscribe.Remove(info);
                                    string id = node.Substring(0, index);
                                    NodeList.UpdateListId.Remove(long.Parse(id));
                                }
                                uDP.Close();
                            }
                        }
                    }
                }
            });

        }


        /// <summary>
        /// 更新列表
        /// </summary>
        public void UpdateList()
        {
            if (NodeList.UpdateListId.Count==0)
            {
                return;
            }
            Task.Factory.StartNew(() =>
            {
                NodeList.UpdateListId.Sort();
                int index = NodeList.UpdateListId.IndexOf(NodeList.UpdateListCurrentID);
                index = (index + 1) % NodeList.UpdateListId.Count;
                
                if (NodeList.UpdateListId[index] == LocalNode.NodeId)
                {
                    //发送列表更新
                    var bytes=  DataPack.PackUpdatePublicList(LocalNode.NodeId, PublishList.Publish.CopyAddress());
                    Multicast.SendTo(bytes);
                    CountUpdateNum = 0;
                }
                else
                {
                    if(CurrentId== NodeList.UpdateListId[index])
                    {
                        //异常，上次也是该节点，计数
                        CountUpdateNum++;

                    }
                    //
                    CurrentId = NodeList.UpdateListId[index];
                    //判断跳过异常是否是自己
                    index = (index + CountUpdateNum) % NodeList.UpdateListId.Count;
                    if (NodeList.UpdateListId[index] == LocalNode.NodeId)
                    {
                        //发送列表更新
                        var bytes = DataPack.PackUpdatePublicList(LocalNode.NodeId, PublishList.Publish.CopyAddress());
                        Multicast.SendTo(bytes);
                        CountUpdateNum = 0;
                    }
                   
                }
            });
          
        }
    
        /// <summary>
        /// 注册节点
        /// </summary>
        public void SendReg()
        {
            AddressInfo info = new AddressInfo();
            info.Reset(LocalNode.TopicAddress);
            Multicast.SendTo(DataPack.PackReg(LocalNode.NodeId,info));
        }
    }
}
