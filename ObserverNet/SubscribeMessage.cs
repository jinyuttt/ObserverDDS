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
using System.Text;
using System.Threading.Tasks;

namespace ObserverNet
{
    public  class SubscribeMessage
    {
        private static readonly Lazy<SubscribeMessage> obj = new Lazy<SubscribeMessage>();

        private readonly UDPSocket uDP = new UDPSocket();
        private readonly TcpServerSocket tcp = new TcpServerSocket();
        private object lock_obj = new object();//
        private bool isInit = false;//是否初始化
        private readonly byte[] rspConst = new byte[] { 1, 1, 1 };

        public static SubscribeMessage Instance
        {
            get { return obj.Value; }
        }
        public string TcpAddress { get; set; }

        public string UdpAddress { get; set; }
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

                TcpAddress = tcp.LocalPoint.Address + "_" + tcp.LocalPoint.Port;
                UdpAddress = uDP.LocalPoint.Address + "_" + uDP.LocalPoint.Port;

            }
        }

        private void Tcp_CallSrv(System.Buffers.ArrayPool<byte> pool, byte[] data,SocketRsp rsp)
        {
            byte[] tmp = new byte[data.Length];
            Array.Copy(data, 0, tmp, 0, tmp.Length);
            pool.Return(data);
            Process(tmp,rsp);
        }

        private void UDP_UDPCall(System.Buffers.ArrayPool<byte> pool, byte[] data,SocketRsp rsp)
        {
            byte[] tmp = new byte[data.Length];
            Array.Copy(data, 0, tmp, 0, tmp.Length);
            pool.Return(data);
            Process(tmp,rsp);
        }

        private void Process(byte[]data, SocketRsp rsp)
        {
            switch(data[0])
            {
                case 2:
                    CopyAddress(data,rsp);
                    break;
                case 3:
                    RspSubscribe(data, rsp);
                    break;
                case 8:
                    RspDetect(data, rsp);
                    break;
                case 0:
                    ProcessTopic(data);
                    break;
            }
        }

        /// <summary>
        /// 返回订阅地址
        /// </summary>
        /// <param name="data"></param>
        private void CopyAddress(byte[] data, SocketRsp rsp)
        {
            string topic = Encoding.Default.GetString(data);
            var addrs = SubscribeList.Subscribe.GetAddresses(topic);
            var bytes = DataPack.PackCopyRspTopic(addrs);
            if(rsp.Rsp!=null)
            {
                rsp.Rsp.Send(bytes);
            }
            else
            {
                uDP.Send(rsp.Address, rsp.Port, bytes);
            }
        }

        /// <summary>
        /// 回复订阅信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rsp"></param>
        private void RspSubscribe(byte[] data, SocketRsp rsp)
        {
            if (rsp.Rsp != null)
            {
                rsp.Rsp.Send(rspConst);
            }
            else
            {
                uDP.Send(rsp.Address, rsp.Port, rspConst);
            }
            //接收订阅
            var msg = DataPack.UnPackSubscribeMsg(data);
            AddressInfo address = new AddressInfo();
            address.Reset(msg.Address);
            SubscribeList.Subscribe.AddAddress(msg.TopicName, new AddressInfo[] { address });
        }

        /// <summary>
        /// 回复侦测
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rsp"></param>
        private void RspDetect(byte[] data, SocketRsp rsp)
        {
            if (rsp.Rsp != null)
            {
                rsp.Rsp.Send(rspConst);
            }
            else
            {
                uDP.Send(rsp.Address, rsp.Port, rspConst);
            }
        }


        private void ProcessTopic(byte[]data)
        {
           var p= DataPack.UnPackTopicData(data);

        }
    }
}
