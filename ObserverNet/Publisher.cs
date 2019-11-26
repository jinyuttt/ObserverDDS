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
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ObserverNet
{
    public  class Publisher
    {
        readonly ArrayPool<byte> poolData = ArrayPool<byte>.Create(1024 * 1024, 10);

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
            if(array==null)
            {
                array =  PublishList.Publish.GetAddresses(topic);
                if(array == null)
                {
                    //没有地址，组播扩展新增
                    NewTopicPub.Pub.SendNewTopic(topic);
                    PublishList.Publish.AddLocal(topic);
                    array = SubscribeList.Subscribe.GetAddresses(topic);
                    if(array!=null)
                    {
                        PubData(array, data);
                    }
                    else
                    {
                        //丢弃
                    }
                }
                else
                {
                    //复制订阅地址
                    List<AddressInfo> lst = null;
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
                                 lst = DataPack.UnPackCopyTopic(buf);
                                SubscribeList.Subscribe.AddAddress(topic, lst.ToArray());
                                tcpClient.Close();
                                break;
                            }
                            tcpClient.Close();
                        }
                        else
                        {
                            UDPSocket uDP = new UDPSocket();
                            bool isSucess = false;
                            for (int i = 0; i < 10; i++)
                            {
                               var tsk= Task.Factory.StartNew(() =>
                                {
                                    uDP.Send(p.Address, p.Port, bytes);
                                    int r = uDP.Recvice(buf);
                                    if (r > 0)
                                    {
                                         lst = DataPack.UnPackCopyTopic(buf);
                                         SubscribeList.Subscribe.AddAddress(topic, lst.ToArray());
                                    }
                                });
                                if(tsk.Wait(100))
                                {
                                    isSucess = true;
                                    break;
                                }

                               
                            }
                            if(isSucess)
                            {
                                break;
                            }
                        }
                    }
                    if(lst!=null)
                    {
                        PubData(lst.ToArray(), data);
                    }
                }

            }
            else
            {
                PubData(array, data);
            }
        }
         
       /// <summary>
       /// 发送数据
       /// </summary>
       /// <param name="lst"></param>
       /// <param name="dat"></param>
        private void   PubData(AddressInfo[] lst, byte[] data)

        {

            foreach (var p in lst)
            {
                if (p.Protol == 0)
                {
                    TcpClientSocket tcp = new TcpClientSocket();
                    tcp.RemoteHost = p.Address;
                    tcp.RemotePort = p.Port;
                    if (tcp.Connect())
                    {
                        tcp.Send(BitConverter.GetBytes(data.Length));
                        tcp.Send(data);
                        tcp.Close();
                    }

                }
                else
                {
                    UDPSocket uDP = new UDPSocket();
                    byte[] buf = poolData.Rent(4 + data.Length);
                    Array.Copy(BitConverter.GetBytes(data.Length), 0, buf, 0, 4);
                    Array.Copy(data, 0, buf, 4, data.Length);
                    uDP.Send(p.Address, p.Port, buf);
                    poolData.Return(buf);
                }
            }
        }
    
    }
}
