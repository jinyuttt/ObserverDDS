#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：MulticastSocket
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
    public delegate void MultCallBuffer(ArrayPool<byte> pool,byte[] data);
   /// <summary>
   /// 组播
   /// </summary>
    public class MulticastSocket
    {
        readonly ArrayPool<byte> poolData = ArrayPool<byte>.Create(1024 * 1024, 100);
        readonly ArrayPool<byte> poolLen = ArrayPool<byte>.Create(4, 1000);
        Socket mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        MulticastOption mcastOption;
        private const string mcastAddress="230.1.1.1";
        IPAddress mcastAddr = IPAddress.Parse(mcastAddress);
        private const int cport = 5555;
        public event MultCallBuffer MulticastCall;

        public void Bind(int port=cport, string host = null)
        {
            mcastOption = new  MulticastOption(mcastAddr, IPAddress.Any);
            if (string.IsNullOrEmpty(host))
            {
                mcastSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            else
            {
                mcastSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            }
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOption);//发送方不必须，接收方必须


        }

        public  int SendTo(byte[]data,int port=cport)
        {
            IPEndPoint ipep = new IPEndPoint(mcastAddr, port);
             return  mcastSocket.SendTo(data, ipep);
        }

        public void Recvice()
        {
            EndPoint point = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] bufLen = poolLen.Rent(4);
                int r = mcastSocket.ReceiveFrom(bufLen, ref point);
                if (r > 0)
                {
                    byte[] buf = poolData.Rent(BitConverter.ToInt32(bufLen, 0));
                    mcastSocket.ReceiveFrom(buf, ref point);
                    MulticastCall(poolData, buf);

                }
                poolLen.Return(bufLen);
               
            }
        }

        public void Close()
        {
            mcastSocket.Close();
        }
    }
}
