using System;
using System.IO;
using System.Threading;

namespace ObserverDDS
{
    //包数+ID+序号+数据
    public  class UDPPack
    {
        private const int Size = 1475-13;
        public const int PackSize = 1475;
     

        
        /// <summary>
        /// 打包发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static UDPPackage Pack(byte[]data)
        {
            int num = data.Length / Size + data.Length % Size>0?1:0;
            int sessionid = UDPPackProcess.Instance.GetID();
            int index = 0;
            byte[] bytesNum = BitConverter.GetBytes(num);
            byte[] bytesSession = BitConverter.GetBytes(sessionid);
            SubPackage[] tmp = new SubPackage[num];
            for (int i=0;i<num;i++)
            {
                SubPackage tp = null;
                if (index + Size < data.Length)
                {
                    tp = new SubPackage() { Data = new byte[Size + 13], SeqId = i, };
                }
                else
                {
                    tp = new SubPackage() { Data = new byte[data.Length-index + 13] , SeqId=i};
                }
                    Array.Copy(bytesNum, 0, tp.Data, 1,4);
                    Array.Copy(bytesSession, 0, tp.Data, 5, 4);
                    Array.Copy(BitConverter.GetBytes(i), 0, tp.Data, 9, 4);
                    Array.Copy(data, index, tp.Data, 13, tp.Data.Length-13);
                   index += Size;
                tmp[i] = tp;
            }
            UDPPackage uDP = new UDPPackage();
            uDP.AddData(tmp);
            return uDP;
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static SubPackage UnPack(byte[] data,int len=0)
        {
            MemoryStream memory = new MemoryStream(data);
            if(len==0)
            {
                len = data.Length - 13;
            }
            byte[] bytesId = new byte[4];
            SubPackage package = new SubPackage();
            package.DataType =(byte) memory.ReadByte();
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

        /// <summary>
        /// 处理回执数据
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static byte[] PackRsp(SubPackage package)
        {
            byte[] rsp = new byte[13];
            MemoryStream memory = new MemoryStream(rsp);
            memory.WriteByte(1);
            memory.Write(BitConverter.GetBytes(package.PackNum),0,4);
            memory.Write(BitConverter.GetBytes(package.SessionId), 0, 4);
            memory.Write(BitConverter.GetBytes(package.SeqId), 0, 4);

            return rsp;

        }

    }
}
