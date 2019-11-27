#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：Publisher
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
namespace ObserverNet
{
    public  class Publisher
    {

        ConcurrentDictionary<string, UDPSocketPack> dicReq = new ConcurrentDictionary<string, UDPSocketPack>();
        ConcurrentDictionary<UDPSocketPack, string> dicReqTopic = new ConcurrentDictionary<UDPSocketPack, string>();
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
            uDP.UDPCall -= UDP_UDPCall;
            uDP.UDPCall += UDP_UDPCall;
            uDP.Send(p.Address, p.Port, bytes);

            
            dicReqTopic[uDP] = topic;
        }

        private void UDP_UDPCall(object sender, byte[] data, SocketRsp rsp)
        {
            UDPSocketPack uDP = sender as UDPSocketPack;
            var  lst = DataPack.UnPackCopyRspTopic(data);
            string topic = dicReqTopic[uDP];
            SubscribeList.Subscribe.AddAddress(topic, lst.ToArray());

            dicReqTopic.TryRemove(uDP, out topic);
            dicReq.TryRemove(topic,out uDP);
            uDP.Close();


        }

        //private void OldCopy()
        //{
        //    // UDPSocket uDP = new UDPSocket();
        //    UDPSocketPack uDP = new UDPSocketPack();
        //    bool isSucess = false;
        //    for (int i = 0; i < 20; i++)
        //    {
        //        var tsk = Task.Factory.StartNew(() =>
        //        {
        //            uDP.Send(p.Address, p.Port, bytes);
        //            int r = uDP.Recvice(buf);
        //            if (r > 0)
        //            {
        //                lst = DataPack.UnPackCopyRspTopic(buf);
        //                SubscribeList.Subscribe.AddAddress(topic, lst.ToArray());
        //            }
        //        });
        //        if (tsk.Wait(50))
        //        {
        //            //成功就退出
        //            isSucess = true;
        //            break;
        //        }


        //    }
        //    if (isSucess)
        //    {
        //        //成功退出外层
        //        break;
        //    }
        //}

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="dat"></param>
        private void PubData(AddressInfo[] lst,string topic, byte[] data)
        {
            byte[] bytes = DataPack.PackTopicData(topic, data);
            foreach (var p in lst)
            {
                if (p.Protol == 0)
                {
                    TcpClientSocket tcp = new TcpClientSocket();
                    tcp.RemoteHost = p.Address;
                    tcp.RemotePort = p.Port;
                    if (tcp.Connect())
                    {
                        tcp.Send(BitConverter.GetBytes(bytes.Length));
                        tcp.Send(bytes);
                        tcp.Close();
                    }

                }
                else
                {
                    //  UDPSocket uDP = new UDPSocket();
                    UDPSocketPack uDP = new UDPSocketPack();
                    uDP.Send(p.Address, p.Port, bytes);
                 
                }
            }
        }
    
    }
}
