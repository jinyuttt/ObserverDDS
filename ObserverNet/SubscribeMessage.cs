#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverNet
* 项目描述 ：
* 类 名 称 ：SubscribeMsg
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
using System.Threading.Tasks;

namespace ObserverNet
{
    public  class SubscribeMessage
    {
        private static readonly Lazy<SubscribeMessage> obj = new Lazy<SubscribeMessage>();
        private UDPSocket uDP = new UDPSocket();
        private TcpServerSocket tcp = new TcpServerSocket();
        private object lock_obj = new object();
        private bool isInit = false;
        public static SubscribeMessage Instance
        {
            get { return obj.Value; }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if(isInit)
            {
                return;
            }
            lock(lock_obj)
            {
                if(isInit)
                {
                    return;
                }
                isInit = true;
                uDP.Bind();
                uDP.UDPCall += UDP_UDPCall;
                Task.Factory.StartNew(() =>
                {
                    uDP.StartRecvice();
                });
                tcp.Bind();
                tcp.CallSrv += Tcp_CallSrv;
             
            }
        }

        private void Tcp_CallSrv(System.Buffers.ArrayPool<byte> pool, byte[] data)
        {
           
        }

        private void UDP_UDPCall(System.Buffers.ArrayPool<byte> pool, byte[] data)
        {
           
        }
    }
}
