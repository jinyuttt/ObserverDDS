#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：UDPSocketPack
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



using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace ObserverDDS
{
    public delegate void UDPSessionCall(object sender,  byte[] data, SocketRsp rsp);
    public  class UDPSocketPack
    {
     
        readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        readonly ConcurrentDictionary<int, UDPPackage> sendQueue = new ConcurrentDictionary<int, UDPPackage>();

        readonly UDPSession recsession = new UDPSession();

        BlockingCollection<RecviceBuffer> block = new BlockingCollection<RecviceBuffer>();

        /// <summary>
        /// 接收数据
        /// </summary>
        readonly ConcurrentQueue<RecviceBuffer> recQueue = new ConcurrentQueue<RecviceBuffer>();

        /// <summary>
        /// 返回数据
        /// </summary>
        readonly ConcurrentQueue<RspBuffer> rspQueue = new ConcurrentQueue<RspBuffer>();

       
        public const int MaxNum = 20;

        public event UDPSessionCall UDPCall;
        public volatile bool isRun = true;
        private volatile bool isStop = false;//是否接收后关闭
        private volatile bool isRecStop = true;
        private volatile bool isSendStop = true;
        private volatile bool isProcessRecStop = true;
        private volatile bool isPspStop = true;
        public IPEndPoint LocalPoint
        {
            get { return (IPEndPoint)socket.LocalEndPoint; }
        }

        /// <summary>
        /// 是否释放
        /// </summary>
        public bool IsDispose
        {
            get { return isRun; }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public void Send(string host, int port, byte[] data)
        {
            var p=  UDPPack.Pack(data);
            p.RemoteHost = new IPEndPoint(IPAddress.Parse(host), port);
            sendQueue[p.SessionId] = p;
            foreach (var k in p.Packages)
            {
                SendTo(k, p.RemoteHost);
            }
            if (isRecStop)
            {
                isRecStop = false;
                StartRecvice();
            }
            if(isSendStop)
            {
                isSendStop = false;
                ReSend();
            }
        }

        private void SendTo(SubPackage package, IPEndPoint point)
        {
            try
            {
                socket.SendTo(package.Data, point);
            }
            catch
            {

            }
        }
        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool Bind(int port = 0, string host = null)
        {
            try
            {
                host = NetAddress.GetLocalIP();
                if (string.IsNullOrEmpty(host))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                }
                else
                {
                    socket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 启动接收
        /// </summary>
        public void StartRecvice()
        {
            isRecStop = false;
            EndPoint point = new IPEndPoint(IPAddress.Any, 0);
            Thread udprec = new Thread(() =>
               {
                   int len = 0;
                   while (isRun)
                   {
                       RecviceBuffer buffer = BufferCache.Instance.GetBuffer();
                       try
                       {
                           len = socket.ReceiveFrom(buffer.Data, ref point);
                       }
                       catch (SocketException ex)
                       {
                           Debug.WriteLine(ex);

                       }
                       IPEndPoint iP = (IPEndPoint)point;
                       buffer.Point = iP;
                       buffer.Len = len;
                       recQueue.Enqueue(buffer);
                       if (isProcessRecStop)
                       {
                           isProcessRecStop = false;
                           ProcessRecvice();
                       }
                   }
                   isRecStop = true;
               });
            udprec.Name = "udpRec";
            udprec.IsBackground = true;
            if (!udprec.IsAlive)
            {
                udprec.Start();
            }

        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        private void ProcessRecvice()
        {
            Task.Factory.StartNew(() =>
            {
                RecviceBuffer buffer = null;
                UDPPackage package;
                while (!recQueue.IsEmpty)
                {
                    if (recQueue.TryDequeue(out buffer))
                    {
                       var p= UDPPack.UnPack(buffer.Data, buffer.Len);
                        if(p.PackNum==0)
                        {
                            continue;
                        }
                        if(p.DataType==1)
                        {
                            if(sendQueue.TryGetValue(p.SessionId,out package))
                            {
                                package.Remove(p.SeqId);
                            }
                        }
                        else
                        {
                            rspQueue.Enqueue(new RspBuffer() { Point = buffer.Point, Package = p });//准备回执
                            if(recsession.AddData(buffer.Point.ToString(), p))
                            {
                               var buf=recsession.GetData(buffer.Point.ToString(),p.SessionId);
                                if(UDPPackProcess.Instance.dicFilter.ContainsKey(buffer.Point.ToString()+","+p.SessionId))
                                {
                                    continue;
                                }
                                else
                                {
                                    UDPPackProcess.Instance.dicFilter[buffer.Point.ToString() + "," + p.SessionId] =DateTime.Now.Second;
                                }
                                if (UDPCall != null)
                                {
                                    UDPCall(this, buf, new SocketRsp() { Address = buffer.Point.Address.ToString(), Port = buffer.Point.Port });
                                }
                                else
                                {
                                    //只能复制
                                    var tmp = new RecviceBuffer
                                    {
                                        Data = new byte[buffer.Data.Length],
                                         Len= buffer.Len
                                };
                                    Array.Copy(buffer.Data, 0, tmp.Data, 0, tmp.Data.Length);
                                  
                                 
                                    block.Add(tmp);
                                }

                            }
                            if(isPspStop)
                            {
                                isPspStop = false;
                                ProcessRsp();
                            }
                        }
                        buffer.Return();
                    }
                }
                isProcessRecStop = true;
            });
        }

        /// <summary>
        /// 处理数据接收回执
        /// </summary>
        private void ProcessRsp()
        {
            Task.Factory.StartNew(() =>
            {
                RspBuffer buffer = null;
              
                while (!rspQueue.IsEmpty)
                {
                    if (rspQueue.TryDequeue(out buffer))
                    {
                        var rsp = UDPPack.PackRsp(buffer.Package);
                        try
                        {
                            socket.SendTo(rsp, buffer.Point);
                        }
                        catch(SocketException ex)
                        {
                            Debug.WriteLine(ex);
                            break;
                        }
                        catch(ObjectDisposedException ex)
                        {
                            Debug.WriteLine(ex);
                            this.sendQueue.Clear();
                            
                            this.recsession.Close();
                            break;
                        }
                    }
                }
                isPspStop = true;
            });
        }

        /// <summary>
        /// 独立接受
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public int Recvice(byte[] buf)
        {
            try
            {
              var  rec=  block.Take();
                Array.Copy(rec.Data, 0, buf, 0, rec.Len);
                return rec.Len;
              //  EndPoint point = new IPEndPoint(IPAddress.Any, 0);
               // return socket.ReceiveFrom(buf, ref point);
            }
            catch
            {
                return -1;
            }

        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            isRun = false;
            socket.Close();
            block.Dispose();
        }

        //接收返回户关闭
        public void Stop()
        {
            isStop = true;
        }

        /// <summary>
        /// 重复发送
        /// </summary>
        private void ReSend()
        {
            Task.Factory.StartNew(() =>
            {
                List<int> lst = new List<int>();
                while (!sendQueue.IsEmpty)
                {
                    foreach (var kv in sendQueue)
                    {
                        var data = kv.Value.Packages;
                        int num = 0;

                        for (int i = 0; i < data.Length; i++)
                        {
                            if (data[i] != null)
                            {
                                SendTo(data[i], kv.Value.RemoteHost);
                            }
                            else
                            {
                                num++;
                            }
                        }
                        if (num == data.Length)
                        {
                            lst.Add(kv.Key);
                        }
                        kv.Value.MaxNum--;
                        if (kv.Value.MaxNum < 0)
                        {
                            lst.Add(kv.Key);
                        }
                    }
                    //
                    UDPPackage package = null;
                    foreach (var k in lst)
                    {
                        sendQueue.TryRemove(k, out package);
                    }
                    Thread.Sleep(50);
                }
                isSendStop = true;
                if(isStop)
                {
                    //已经停止，处理完发送后关闭
                    Close();
                }
            });
        }
    }
}
