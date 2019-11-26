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
using System.Threading.Tasks;

namespace ObserverNet
{
    public delegate void MultCallBuffer(ArrayPool<byte> pool,byte[] data,int len);
   /// <summary>
   /// 组播
   /// </summary>
    public class MulticastSocket
    {
        readonly ArrayPool<byte> poolData = ArrayPool<byte>.Create(1024 * 1024, 100);
      //  readonly ArrayPool<byte> poolLen = ArrayPool<byte>.Create(4, 1000);
        Socket mcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        MulticastOption mcastOption;
        private const string mcastAddress="230.1.1.1";
        IPAddress mcastAddr = IPAddress.Parse(mcastAddress);
        private const int cport = 5555;
        public event MultCallBuffer MulticastCall;

        public void Bind(int port=cport, string host = null)
        {
            mcastOption = new  MulticastOption(mcastAddr);
            mcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            if (string.IsNullOrEmpty(host))
            {
                //host = NetAddress.GetLocalIP();
                //if (string.IsNullOrEmpty(host))
                //{
                    mcastSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                //}
                //else
                //{
                //    mcastSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
                //}
            }
            else
            {
                mcastSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            }
            //
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
           
            mcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOption);//发送方不必须，接收方必须


        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public  int SendTo(byte[]data,int port=cport)
        {
             IPEndPoint ipep = new IPEndPoint(mcastAddr, port);
             
             return  mcastSocket.SendTo(data, ipep);
        }

        public void Recvice()
        {
            Task.Factory.StartNew(() =>
            {
                int r = 0;
               EndPoint point = new IPEndPoint(IPAddress.Any, 0);
                while (true)
                {
                    //   byte[] bufLen = poolLen.Rent(1024);
                    //  int r = mcastSocket.ReceiveFrom(bufLen, ref point);
                    //  if (r > 0)
                    //  {
                    byte[] buf = poolData.Rent(1024);
                    r = mcastSocket.ReceiveFrom(buf, ref point);
                    byte[] tmp = poolData.Rent(r);
                    Array.Copy(buf, tmp, r);
                    poolData.Return(buf);
                    MulticastCall(poolData, tmp,r);

                    //  }
                    //  poolLen.Return(bufLen);

                }
            });
         
        }

        public void Close()
        {
            mcastSocket.Close();
        }
    }
}
