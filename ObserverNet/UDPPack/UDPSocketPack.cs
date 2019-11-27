#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：UDPSocketPack
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



using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace ObserverNet
{
    public delegate void UDPSessionCall(object sender,  byte[] data, SocketRsp rsp);
    public  class UDPSocketPack
    {
     
        readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        readonly ConcurrentDictionary<int, UDPPackage> sendQueue = new ConcurrentDictionary<int, UDPPackage>();
        readonly UDPSession recsession = new UDPSession();
        readonly ConcurrentQueue<RecviceBuffer> recQueue = new ConcurrentQueue<RecviceBuffer>();
        readonly ConcurrentQueue<RspBuffer> rspQueue = new ConcurrentQueue<RspBuffer>();
        readonly ConcurrentDictionary<string, string> dicFilter = new ConcurrentDictionary<string, string>();
        public const int MaxNum = 20;

        public event UDPSessionCall UDPCall;
        public volatile bool isRun = true;
        private volatile bool isRecStop = true;
        private volatile bool isSendStop = true;
        private volatile bool isProcessRecStop = true;

        public IPEndPoint LocalPoint
        {
            get { return (IPEndPoint)socket.LocalEndPoint; }
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
                isRecStop = false;
                ReSend();
            }
        }

        private void SendTo(SubPackage package, IPEndPoint point)
        {
            socket.SendTo(package.Data, point);
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
        /// 
        /// </summary>
        public void StartRecvice()
        {
            isRecStop = false;
            EndPoint point = new IPEndPoint(IPAddress.Any, 0);
            int len = 0;
            while (isRun)
            {

                byte[] buf = new byte[UDPPack.PackSize];
                try
                {
                    len = socket.ReceiveFrom(buf, ref point);
                }
                catch(SocketException ex)
                {
                    break;
                }
                IPEndPoint iP = (IPEndPoint)point;
                recQueue.Enqueue(new RecviceBuffer() {  Point=iP, Data = buf,   Len=len});
                if(isProcessRecStop)
                {
                    isProcessRecStop = false;
                    ProcessRecvice();
                }
            }

        }

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
                        if(p.DataType==1)
                        {
                            if(sendQueue.TryGetValue(p.SessionId,out package))
                            {
                                package.Remove(p.SeqId);
                            }
                        }
                        else
                        {
                            rspQueue.Enqueue(new RspBuffer() { Point = buffer.Point, Package = p });
                            if(recsession.AddData(buffer.Point.ToString()+buffer.Point.Port, p))
                            {
                               var buf=recsession.GetData(buffer.Point.ToString() + buffer.Point.Port,p.SessionId);
                                if(dicFilter.ContainsKey(buffer.Point.ToString() + buffer.Point.Port+","+p.SessionId))
                                {
                                    continue;
                                }
                                UDPCall(this,buf, new SocketRsp() { Address = buffer.Point.ToString(), Port = buffer.Point.Port });
                            }
                           
                        }
                    }
                }
                isProcessRecStop = true;
            });
        }

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
                        socket.SendTo(rsp, buffer.Point);
                    }
                }
            });
        }

        /// <summary>
        /// 独立接受
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public int Recvice(byte[] buf)
        {
            EndPoint point = new IPEndPoint(IPAddress.Any, 0);

            return socket.ReceiveFrom(buf, ref point);


        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            isRun = false;
            socket.Close();
        }

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
            });
        }
    }
}
