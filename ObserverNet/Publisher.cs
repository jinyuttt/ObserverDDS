﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：Publisher
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
using System.Threading;

namespace ObserverDDS
{

    /// <summary>
    /// 数据发布
    /// </summary>
    public  class Publisher
    {
        readonly ConcurrentDictionary<string, UDPSocketPack> dicReq = new ConcurrentDictionary<string, UDPSocketPack>();

        readonly ConcurrentDictionary<UDPSocketPack, string> dicReqTopic = new ConcurrentDictionary<UDPSocketPack, string>();

        /// <summary>
        /// 用于发布的socket
        /// </summary>
        BlockingCollection<UDPSocketPack> udpSocket = new BlockingCollection<UDPSocketPack>(System.Environment.ProcessorCount/2);
        
        private  int UdpSocketNum = System.Environment.ProcessorCount/2;

        private static readonly Lazy<Publisher> obj = new Lazy<Publisher>();

        public static Publisher Instance
        {
            get { return obj.Value; }
        }


       /// <summary>
       /// 发布数据
       /// </summary>
       /// <param name="topic"></param>
       /// <param name="data"></param>
        public void Publish(string topic,byte[]data)
        {
            if(ObserverInit.isInit)
            {
                ObserverInit.Init();
            }
            var array= SubscribeList.Subscribe.GetAddresses(topic);
            if(array==null||array.Length==0)
            {
                //没有订阅看发布
                array =  PublishList.Publish.GetAddresses(topic);
                if(array == null||(array.Length==1&&array[0].ToString()==LocalNode.TopicAddress))
                {
                    //没有地址，或者只是本节点发布了数据，当做新增，组播扩展新增
                    NewTopicPub.Pub.SendNewTopic(topic);
                    PublishList.Publish.AddLocal(topic);
                    array = SubscribeList.Subscribe.GetAddresses(topic);
                    if(array!=null)
                    {
                        //再次订阅的地址
                        PubData(array,topic, data);
                    }
                    else
                    {
                        //丢弃
                    }
                }
                else
                {
                    //复制订阅地址
                    List<AddressInfo> lst = null;//地址
                    foreach (var p in array)
                    {
                        var bytes = DataPack.PackCopyTopic(topic);
                        byte[] buf = new byte[10240];
                       
                        if (p.Protol == 0)
                        {
                            TcpClientSocket tcpClient = new TcpClientSocket();
                            tcpClient.RemoteHost = p.Address;
                            tcpClient.RemotePort = p.Port;
                            if (tcpClient.Connect())
                            {

                                tcpClient.Send(bytes);
                                int r = tcpClient.Recvice(buf);
                                 lst = DataPack.UnPackCopyRspTopic(buf);
                                SubscribeList.Subscribe.AddAddress(topic, lst.ToArray());
                                tcpClient.Close();
                                break;//成功一个就退出
                            }
                            tcpClient.Close();
                        }
                        else
                        {

                            RequestCopy(topic,p, bytes);
                           
                        }
                    }
                    if(lst!=null)
                    {
                        PubData(lst.ToArray(),topic, data);
                    }
                }

            }
            else
            {
                PubData(array,topic, data);
            }
        }

        /// <summary>
        /// 请求复制
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="p"></param>
        /// <param name="bytes"></param>
        private void RequestCopy(string topic,AddressInfo p,byte[] bytes)
        {
         
            UDPSocketPack uDP = new UDPSocketPack();
            uDP= dicReq.GetOrAdd(topic,  uDP);
            dicReqTopic[uDP] = topic;
            uDP.UDPCall -= UDP_UDPCall;
            uDP.UDPCall += UDP_UDPCall;
            uDP.Send(p.Address, p.Port, bytes);
        }

        private void UDP_UDPCall(object sender, byte[] data, SocketRsp rsp)
        {
            string topic;
            UDPSocketPack uDP = sender as UDPSocketPack;
            if (dicReqTopic.TryRemove(uDP, out topic))
            {
                var lst = DataPack.UnPackCopyRspTopic(data);
               
                SubscribeList.Subscribe.AddAddress(topic, lst.ToArray());

                dicReq.TryRemove(topic, out uDP);
                uDP.Close();
            }
        }

      
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="dat"></param>
        private void PubData(AddressInfo[] lst,string topic, byte[] data)
        {
            byte[] bytes = DataPack.PackTopicData(topic, data);
            bool findTcp = false;
            List<AddressInfo> lstTcp = new List<AddressInfo>();
            foreach (var p in lst)
            {
                if (p.Protol == 0)
                {
                    if(findTcp)
                    {
                        if(lstTcp.Contains(p))
                        {
                            continue;//已经处理
                        }
                        else
                        {
                            TcpClientSocket tcp = new TcpClientSocket
                            {
                                RemoteHost = p.Address,
                                RemotePort = p.Port
                            };
                            if (tcp.Connect())
                            {
                                tcp.Send(BitConverter.GetBytes(bytes.Length));
                                tcp.Send(bytes);
                                ClientProcess.Instance.Update(topic,tcp);
                                //tcp.Close();
                            }
                        }
                        continue;
                    }
                   
                    var lstSocket = ClientProcess.Instance.GetTcpClients(topic);
                    if(lstSocket!=null)
                    {
                        lock (lstSocket)
                        {
                            //必须锁定防止处理线程正在关闭
                            foreach (var tcpClient in lstSocket)
                            {
                                try
                                {
                                    tcpClient.Send(bytes);
                                    lstTcp.Add(new AddressInfo() { Address = tcpClient.RemoteHost, Port = tcpClient.RemotePort, Protol = 0 });
                                }
                                catch
                                {
                                    //有可能正在关闭；
                                }
                            }
                        }
                    }
                    //当前第一个地址
                    if(!lstTcp.Contains(p))
                    {
                        TcpClientSocket tcp = new TcpClientSocket
                        {
                            RemoteHost = p.Address,
                            RemotePort = p.Port
                        };
                        if (tcp.Connect())
                        {
                            tcp.Send(BitConverter.GetBytes(bytes.Length));
                            tcp.Send(bytes);
                            ClientProcess.Instance.Update(topic, tcp);
                            //tcp.Close();
                        }
                    }
                    findTcp = true;//已经完全处理过；



                }
                else
                {
                    //  UDPSocket uDP = new UDPSocket();
                    UDPSocketPack uDP = null;

                    if (udpSocket.TryTake(out uDP))
                    {
                        uDP.Send(p.Address, p.Port, bytes);
                    }
                    else
                    {
                        if (Interlocked.Decrement(ref UdpSocketNum) > 0)
                        {
                            uDP = new UDPSocketPack();
                            uDP.Send(p.Address, p.Port, bytes);
                        }
                        else
                        {
                            uDP= udpSocket.Take();
                            uDP.Send(p.Address, p.Port, bytes);
                        }
                    }
                    if(!udpSocket.TryAdd(uDP))
                    {
                        uDP.Stop();
                    }
                   
                }
            }
        }
    
    }
}
