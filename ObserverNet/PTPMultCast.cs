using System;

namespace ObserverDDS
{

    /// <summary>
    /// 点对点通信交互组播通信内容
    /// 替换无法组播通信的网段,相当于桥接功能
    /// 
    /// </summary>
    public   class PTPMultCast
    {
        public static bool IsPtP = false;

        private static Lazy<PTPMultCast> obj = new Lazy<PTPMultCast>();

        public static PTPMultCast Instance
        {
            get { return obj.Value; }
        }

        /// <summary>
        /// 向远端发送地址
        /// </summary>
        public AddressInfo RemoteAddress { get; set; }

        /// <summary>
        /// 本地接收地址
        /// </summary>
        public AddressInfo LocalAddress { get; set; }

        public  void Start()
        {
            if(IsPtP&&LocalAddress!=null)
            {
               if(LocalAddress.Protol==0)
                {
                    TcpServerSocket tcpServer = new TcpServerSocket();
                    tcpServer.Bind(LocalAddress.Port);
                    tcpServer.CallSrv += TcpServer_CallSrv;
                }
               else
                {
                    UDPSocketPack uDP = new UDPSocketPack();
                    uDP.Bind(LocalAddress.Port, LocalAddress.Address);
                    uDP.UDPCall += UDP_UDPCall;
                }
            }
        }

        private void UDP_UDPCall(object sender, byte[] data, SocketRsp rsp)
        {
            NodeListener.Instance.Process(data, data.Length);
           
        }

        private void TcpServer_CallSrv(System.Buffers.ArrayPool<byte> pool, byte[] data, int len, SocketRsp rsp)
        {
            //接收组播信息
            NodeListener.Instance.Process(data, len);
            pool.Return(data);
        }

        public void Send(byte[] data)
        {
            if (!IsPtP || RemoteAddress == null)
            {
                return;
            }
            if (RemoteAddress.Protol == 0)
            {
                TcpClientSocket   tcpClient= new TcpClientSocket() { RemoteHost = RemoteAddress.Address, RemotePort = RemoteAddress.Port };
                if(tcpClient.Connect())
                {
                    tcpClient.Send(data);
                }
                tcpClient.Close();
            }
            else
            {
                UDPSocketPack uDP = new UDPSocketPack();
                uDP.Send(RemoteAddress.Address, RemoteAddress.Port, data);
                uDP.Stop();
            }
        }
       
    }
}
