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

namespace ObserverNet
{
    public delegate void CallRsp(ArrayPool<byte> pool,byte[] data,SocketRsp rsp);
    public  class TcpServerSocket
    {
        readonly ArrayPool<byte> poolData = ArrayPool<byte>.Create(1024 * 1024, 100);
        readonly ArrayPool<byte> poolLen = ArrayPool<byte>.Create(4, 1000);

        public event CallRsp CallSrv;

        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Bind(int port=0)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                Thread threadReceive = new Thread(new ParameterizedThreadStart(StartListen));
                threadReceive.IsBackground = true;
                threadReceive.Start(socket);
               
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
            while (true)
            {
                //客户端连接成功后，服务器接收客户端发送的消息
                //byte[] buffer = new byte[2048];
                //实际接收到的有效字节数
                byte[] bufLen = poolLen.Rent(4);
                int count = socketSend.Receive(bufLen);
                if (count == 0)//count 表示客户端关闭，要退出循环
                {
                    break;
                }
                else
                {
                    byte[] buf = poolData.Rent(BitConverter.ToInt32(bufLen, 0));
                    socketSend.Receive(buf);
                    CallSrv(poolData, buf,new SocketRsp() { Rsp = socketSend });
                 
                }
                poolLen.Return(bufLen);
            }
        }


    }
}
