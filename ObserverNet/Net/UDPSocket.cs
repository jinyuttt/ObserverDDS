#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：UDPSocket
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

namespace ObserverNet
{
    public delegate void UDPCallBuffer(ArrayPool<byte> pool, byte[] data,int len,SocketRsp rsp);
    public class UDPSocket
    {
         ArrayPool<byte> poolData = null;
        readonly ArrayPool<byte> poolLen = ArrayPool<byte>.Create(4, 1000);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public event UDPCallBuffer UDPCall;

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
        public int Send(string host,int port,byte[]data)
        {
           return socket.SendTo(data, new IPEndPoint(IPAddress.Parse(host), port));
        }


        /// <summary>
        /// 绑定
        /// </summary>
        /// <param name="port"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public bool Bind(int port=0, string host=null)
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
            poolData = ArrayPool<byte>.Create(1024 * 1024, 100);
            EndPoint point = new IPEndPoint(IPAddress.Any,0);
            int r = 0;
            while (true)
            {
                byte[] buf = poolLen.Rent(1024);
             
                    r=socket.ReceiveFrom(buf, ref point);
                    IPEndPoint iP = (IPEndPoint)point;
                    UDPCall(poolData, buf,r,new SocketRsp() { Address = iP.Address.ToString(), Port = iP.Port });
              
            }

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
            
            socket.Close();
        }
    }
}
