using System;
using System.IO;

namespace ObserverNet
{
    //包数+ID+序号+数据
    public  class UDPPack
    {
        private const int Size = 1475-12;
        public static UDPPackage Pack(byte[]data)
        {
            int num = data.Length / Size + data.Length % Size;
            int sessionid = 0;
          
           
            int index = 0;
            byte[] bytesNum = BitConverter.GetBytes(num);
            byte[] bytesSession = BitConverter.GetBytes(sessionid);
            SessionPackage[] tmp = new SessionPackage[num];
            for (int i=0;i<num;i++)
            {
                SessionPackage tp = null;
                if (index + Size > data.Length)
                {
                    tp = new SessionPackage() { Data = new byte[Size + 12], SeqId = i, };
                }
                else
                {
                    tp = new SessionPackage() { Data = new byte[data.Length-index + 12] , SeqId=i};
                }
                    Array.Copy(bytesNum, 0, tp.Data, 0,4);
                    Array.Copy(bytesSession, 0, tp.Data, 4, 4);
                    Array.Copy(BitConverter.GetBytes(i), 0, tp.Data, 8, 4);
                    Array.Copy(data, index, tp.Data, 12, tp.Data.Length);
                   index += Size;
                tmp[i] = tp;
            }
            UDPPackage uDP = new UDPPackage();
            uDP.AddData(tmp);
            return uDP;
        }

        public static SessionPackage UnPack(byte[] data,int len=0)
        {
            MemoryStream memory = new MemoryStream(data);
            if(len==0)
            {
                len = data.Length - 12;
            }
            byte[] bytesId = new byte[4];
            SessionPackage package = new SessionPackage();
            memory.Read(bytesId, 0, 4);
            package.PackNum = BitConverter.ToInt32(bytesId, 0);
            memory.Read(bytesId, 0, 4);
            package.SessionId = BitConverter.ToInt32(bytesId, 0);
            memory.Read(bytesId, 0, 4);
            package.SeqId = BitConverter.ToInt32(bytesId, 0);
            //
            memory.Read(package.Data = new byte[len], 0, len);

            memory.Close();
            return package;
        }


    }
}
