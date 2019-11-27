#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：TcpServerSocket
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ObserverNet
{
    public delegate void CallRsp(ArrayPool<byte> pool,byte[] data,int len,SocketRsp rsp);
    public  class TcpServerSocket
    {
        readonly ArrayPool<byte> poolData = ArrayPool<byte>.Create(1024 * 1024, 100);
        readonly ConcurrentQueue<byte[]> poolLen = new ConcurrentQueue<byte[]>();

        public event CallRsp CallSrv;


        public IPEndPoint LocalPoint
        {
            get;set;
        }
        private void Init()
        {
            for(int i=0;i<1000;i++)
            {
                poolLen.Enqueue(new byte[4]);
            }
        }
        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Bind(int port=0)
        {
            try
            {
                string host = NetAddress.GetLocalIP();
                IPAddress address = IPAddress.Any;
                if(!string.IsNullOrEmpty(host))
                {
                    address = IPAddress.Parse(host);
                }
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(address, port));
                socket.Listen(100);
                Thread threadReceive = new Thread(new ParameterizedThreadStart(StartListen));
                threadReceive.IsBackground = true;
                threadReceive.Start(socket);
                LocalPoint =(IPEndPoint) socket.LocalEndPoint;
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 监听
        /// </summary>
        /// <param name="obj"></param>
        private void StartListen(object obj)
        {
            Socket socketWatch = obj as Socket;
            Init();
            while (true)
            {

                //等待客户端的连接，并且创建一个用于通信的Socket
                Socket socketSend = socketWatch.Accept();
                //获取远程主机的ip地址和端口号

                //定义接收客户端消息的线程
                Task.Factory.StartNew(new Action<object>(Receive), socketSend);

            }
        }
        /// <summary>
        /// 服务器端不停的接收客户端发送的消息
        /// </summary>
        /// <param name="obj"></param>
        private void Receive(object obj)
        {
            Socket socketSend = obj as Socket;
            int r = 0;
            while (true)
            {
                //客户端连接成功后，服务器接收客户端发送的消息
                //byte[] buffer = new byte[2048];
                //实际接收到的有效字节数
                int len = 0;
                byte[] bufLen = null;
                if (poolLen.TryDequeue(out bufLen))
                {
                    int count = 0;
                    try
                    {
                        count= socketSend.Receive(bufLen);
                    }
                    catch(SocketException ex)
                    {
                        poolLen.Enqueue(bufLen);
                        break;
                    }
                    if (count == 0)//count 表示客户端关闭，要退出循环
                    {
                        break;
                    }
                    else
                    {
                        len = BitConverter.ToInt32(bufLen, 0);
                         byte[] buf = poolData.Rent(len);
                      
                            r = socketSend.Receive(buf);

                        
                        CallSrv(poolData, buf, r, new SocketRsp() { Rsp = socketSend });

                    }
                    poolLen.Enqueue(bufLen);
                }
            }
        }


    }
}
