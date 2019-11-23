﻿#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：TcpClientSocket
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
using System.Net.Sockets;

namespace ObserverNet
{
    public class TcpClientSocket
    {
        readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public string RemoteHost { get; set; }

        public int RemotePort { get; set; }

        public bool Connect()
        {
            try
            {
               // socket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.SendTimeout,)
                socket.Connect(RemoteHost, RemotePort);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public  int Send(byte[] data)
        {
          return  socket.Send(data);
        }

        public int  Recvice(byte[] buf)
        {
           return socket.Receive(buf);
        }

       public void Close()
        {
            socket.Close();
        }
    }
}
