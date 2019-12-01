#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：DataPack
* 类 描 述 ：
* 所在的域 ：DESKTOP-1IBOINI
* 命名空间 ：ObserverDDS
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ObserverDDS
{

   /// <summary>
   /// 数据组包解包
   /// </summary>
  public  class DataPack
    {

        /// <summary>
        /// 新增主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="nodeId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static byte[] PackNewTopic(string topic,long nodeId,string address)
        {
            byte[] topics = Encoding.Default.GetBytes(topic);
            byte[] addr = Encoding.Default.GetBytes(address);
            byte[] topicLen = BitConverter.GetBytes(topics.Length);
            byte[] len = BitConverter.GetBytes(topics.Length+addr.Length+4);
            byte[] bytes = new byte[topics.Length + addr.Length + topicLen.Length + len.Length+8];
            //
            Array.Copy(len, bytes, 4);//总长
            Array.Copy(topicLen, 0, bytes, 4, 4);//主题长度
            Array.Copy(topics, 0, bytes, 8, topics.Length);//主题
            Array.Copy(BitConverter.GetBytes(nodeId), 0, bytes, 8+ topics.Length,8);//id
            Array.Copy(addr, 0, bytes, topics.Length+16, addr.Length);//地址
            byte[] tmp = new byte[bytes.Length + 1];
            tmp[0] = 1;
            Array.Copy(bytes, 0, tmp, 1, bytes.Length);
            return tmp;
        }


        /// <summary>
        /// 新增发布主题
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TopicMessage UnPackNewTopic(byte[]data,int count)
        {
            TopicMessage message = new TopicMessage();
            byte[] tmp = new byte[count-1];//除去数据标识
            Array.Copy(data, 1, tmp,0, tmp.Length);
            Span<byte> bytes = tmp;
            var lenSP=  bytes.Slice(0, 4);
            int len = BitConverter.ToInt32(lenSP.ToArray(), 0);
            lenSP = bytes.Slice(4, 4);
            int topicLen = BitConverter.ToInt32(lenSP.ToArray(), 0);
            lenSP = bytes.Slice(8, topicLen);
            string topic = Encoding.Default.GetString(lenSP.ToArray());
            lenSP = bytes.Slice(8+ topicLen, 8);
           long id= BitConverter.ToInt32(lenSP.ToArray(), 0);
            lenSP = bytes.Slice(16 + topicLen, count-1 - 16- topicLen);
            string addr= Encoding.Default.GetString(lenSP.ToArray());
            message.Address = addr;
            message.NodeId = id;
            message.TopicName = topic;
            return message;
        }

        /// <summary>
        /// 复制订阅地址
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public static byte[] PackCopyTopic(string topic)
        {
            byte[] topics = Encoding.Default.GetBytes(topic);
            byte[] tmp = new byte[topics.Length +1];
            Array.Copy(topics, 0, tmp, 1, topics.Length );
            tmp[0] = 2;
            return tmp;
        }

        /// <summary>
        /// 复制订阅地址发送
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public static byte[] PackCopyRspTopic(AddressInfo[] addresses)
        {
           
            List<byte> lst = new List<byte>();
            lst.Add(5);
            if (addresses != null)
            {
                lst.AddRange(BitConverter.GetBytes(addresses.Length));
                foreach (var p in addresses)
                {
                    byte[] tmp = Encoding.Default.GetBytes(p.ToString());
                    lst.AddRange(BitConverter.GetBytes(tmp.Length));
                    lst.AddRange(tmp);
                }
            }
          
            return lst.ToArray();
        }

        /// <summary>
        /// 返回订阅地址
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<AddressInfo> UnPackCopyRspTopic(byte[]data)
        {
            byte[] tmpSum = new byte[data.Length - 1];
            Array.Copy(data, 1, tmpSum, 0, data.Length - 1);
            Span<byte> span = tmpSum;
            int index = 0;
            List<AddressInfo> lst = new List<AddressInfo>();
            if (tmpSum.Length > 0)
            {
                var sp = span.Slice(0, 4);
                int num = BitConverter.ToInt32(sp.ToArray(), 0);
                index += 4;
                for (int i = 0; i < num; i++)
                {

                    sp = span.Slice(index, 4);
                    int len = BitConverter.ToInt32(sp.ToArray(), 0);
                    sp = span.Slice(index, len);
                    string tmp = Encoding.Default.GetString(sp.ToArray());
                    AddressInfo address = new AddressInfo();
                    string[] info = tmp.Split('_');
                    address.Address = info[1];
                    address.Port = int.Parse(info[2]);
                    address.Protol = int.Parse(info[0]);
                    lst.Add(address);
                    index += 4;
                    index += len;
                }
            }
            return lst;

        }

        /// <summary>
        /// 发送订阅信息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public static byte[] PackSubscribeMsg(string topic,AddressInfo[] local)
        {
            List<byte> lst = new List<byte>();
            StringBuilder sbr = new StringBuilder();
            //总长+主题长度+主题+地址(;)
            byte[] bytes = Encoding.Default.GetBytes(topic);
            foreach(var addr in local)
            {
                sbr.Append(addr.ToString());
                sbr.Append(";");
            }
            sbr.Remove(sbr.Length - 1, 1);
            byte[] address = Encoding.Default.GetBytes(sbr.ToString());
            int Len = bytes.Length + address.Length + 4;
            lst.AddRange(BitConverter.GetBytes(Len));//总长
            lst.AddRange(BitConverter.GetBytes(bytes.Length));//主题长度
            lst.AddRange(bytes);
            lst.AddRange(address);
            lst.Insert(0, 3);
            return lst.ToArray();
        }


        /// <summary>
        /// 解析订阅信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TopicMessage UnPackSubscribeMsg(byte[]data)
        {
            byte[] tmp = new byte[data.Length - 1];
            Array.Copy(data, 1, tmp, 0, data.Length - 1);
            Span<byte> span = tmp;
            var sp= span.Slice(0, 4);
            int len = BitConverter.ToInt32(sp.ToArray(), 0);
            sp = span.Slice(4, 4);
            int topicLen= BitConverter.ToInt32(sp.ToArray(), 0);
            sp = span.Slice(8, topicLen);
            string topic = Encoding.Default.GetString(sp.ToArray());
            sp = span.Slice(8+ topicLen);
            string addr = Encoding.Default.GetString(sp.ToArray());
            TopicMessage message = new TopicMessage() { Address = addr, TopicName = topic };
            return message;
        }


        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="nodeid"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static byte[] PackNodeState(long nodeid, string address)
        {
            string msg = string.Format("{0},{1}", nodeid, address);
            List<byte> lst = new List<byte>();
            lst.Add(6);
            lst.AddRange(Encoding.Default.GetBytes(msg));
            return lst.ToArray();
        }


        /// <summary>
        /// 节点心跳
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string UnPackNodeState(byte[] data,int count)
        {
            byte[] tmp = new byte[count - 1];
            Array.Copy(data, 1, tmp, 0, count - 1);
            return Encoding.Default.GetString(tmp);
        }

       /// <summary>
       /// 更新数据列表
       /// </summary>
       /// <param name="data"></param>
       /// <returns></returns>
        public static Dictionary<string,List<AddressInfo>>  UnPackUpdatePublicList(byte[]data,int count,out long nodeid)
        {
            byte[] len = new byte[2];
            byte[] tmp = new byte[1024];
            Dictionary<string, List<AddressInfo>> dic = new Dictionary<string, List<AddressInfo>>();
            MemoryStream memory = new MemoryStream(data);
            memory.ReadByte();//去除标识

            List<AddressInfo> lst = null;
            byte[] bufID = new byte[8];
            memory.Read(bufID, 0, 8);
            nodeid = BitConverter.ToInt64(bufID,0);
            while (memory.Position < count)
            {
                lst = new List<AddressInfo>();
                memory.Read(len, 0, 2);
                short infoLen = BitConverter.ToInt16(len, 0);
                memory.Read(len, 0, 2);
                short topicLen = BitConverter.ToInt16(len, 0);
                memory.Read(tmp, 0, topicLen);
                string topic = Encoding.Default.GetString(tmp, 0, topicLen);
                short addrLen = (short)(infoLen - topicLen - 2);
                if (addrLen > 1024)
                {
                    tmp = new byte[addrLen];
                }
                string str = null;
                memory.Read(tmp, 0, addrLen);
                str = Encoding.Default.GetString(tmp, 0, addrLen);
                string[] address = str.Split(';');
                foreach(var p in address)
                {
                    AddressInfo addr = new AddressInfo();
                    addr.Reset(p);
                    lst.Add(addr);
                }
                dic[topic] = lst;
            }
            memory.Close();
            return dic;
        }

        /// <summary>
        /// 更新数据列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] PackUpdatePublicList(long nodeid,Dictionary<string,List<AddressInfo>> pairs)
        {
            List<byte> lst = new List<byte>();
            lst.Add(7);
            lst.AddRange(BitConverter.GetBytes(nodeid));
            StringBuilder builder = new StringBuilder();
            foreach(var kv in pairs)
            {
                builder.Clear();
                byte[] topic = Encoding.Default.GetBytes(kv.Key);
                foreach(var addr in kv.Value)
                {
                    builder.AppendFormat("{0};", addr);
                }
                builder.Remove(builder.Length - 1, 1);
                byte[] tmp = Encoding.Default.GetBytes(builder.ToString());
                //
                lst.AddRange(BitConverter.GetBytes((short)(topic.Length + tmp.Length + 2)));
                lst.AddRange(BitConverter.GetBytes((short)(topic.Length)));
                lst.AddRange(topic);
                lst.AddRange(tmp);
            }
            return lst.ToArray();
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="nodeid"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static byte[] PackReg(long nodeid,AddressInfo info)
        {
            string msg = string.Format("{0},{1}", nodeid, info);
            List<byte> lst = new List<byte>();
            lst.Add(9);
            lst.AddRange( Encoding.Default.GetBytes(msg));
            return lst.ToArray();
        }
        
        /// <summary>
        /// 解析注册
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static  string UnPackReg(byte[]data,int count)
        {
            byte[] tmp = new byte[count - 1];
            Array.Copy(data, 1, tmp, 0, count - 1);
            return Encoding.Default.GetString(tmp);
        }

        /// <summary>
        /// 打包数据
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] PackTopicData(string topic,byte[]data)
        {
            List<byte> lst = new List<byte>(data.Length + 20);
            byte[] tp = Encoding.Default.GetBytes(topic);
            lst.Add(0);
            lst.AddRange(BitConverter.GetBytes((short)(tp.Length)));
            lst.AddRange(tp);
            lst.AddRange(data);
            return lst.ToArray();
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TopicData UnPackTopicData(byte[]data)
        {
            TopicData topic = new TopicData();
            MemoryStream memory = new MemoryStream(data);
            memory.ReadByte();
            byte[] len = new byte[2];
            memory.Read(len, 0, 2);
            short tlen = BitConverter.ToInt16(len, 0);
            byte[] topicbytes = new byte[tlen];
            memory.Read(topicbytes, 0, tlen);
            string str = Encoding.Default.GetString(topicbytes);
            byte[] tmp = new byte[data.Length - 2 - tlen];
            memory.Read(tmp, 0, tmp.Length);
            topic.TopicName = str;
            topic.Data = tmp;
            return topic;

        }

        /// <summary>
        /// 新增发布主题回复
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TopicMessage UnPackNewTopicRsp(byte[] data, int count)
        {
            TopicMessage message = new TopicMessage();
            byte[] tmp = new byte[count - 1];
            Array.Copy(data, 1, tmp, 0, tmp.Length);
            Span<byte> bytes = tmp;
            var lenSP = bytes.Slice(0, 4);
            int len = BitConverter.ToInt32(lenSP.ToArray(), 0);
            lenSP = bytes.Slice(4, 4);
            int topicLen = BitConverter.ToInt32(lenSP.ToArray(), 0);
            lenSP = bytes.Slice(8, topicLen);
            string topic = Encoding.Default.GetString(lenSP.ToArray());
            lenSP = bytes.Slice(8 + topicLen, 8);
            long id = BitConverter.ToInt32(lenSP.ToArray(), 0);
            lenSP = bytes.Slice(16 + topicLen, count-1 - 16 - topicLen);
            string addr = Encoding.Default.GetString(lenSP.ToArray());
            message.Address = addr;
            message.NodeId = id;
            message.TopicName = topic;
            return message;
        }
       
        /// <summary>
        /// 新增主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="nodeId"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public static byte[] PackNewTopicRsp(string topic, long nodeId, string address)
        {
            byte[] topics = Encoding.Default.GetBytes(topic);
            byte[] addr = Encoding.Default.GetBytes(address);
            byte[] topicLen = BitConverter.GetBytes(topics.Length);
            byte[] len = BitConverter.GetBytes(topics.Length + addr.Length + 4);
            byte[] bytes = new byte[topics.Length + addr.Length + topicLen.Length + len.Length + 8];
            //
            Array.Copy(len, bytes, 4);//总长
            Array.Copy(topicLen, 0, bytes, 4, 4);//主题长度
            Array.Copy(topics, 0, bytes, 8, topics.Length);//主题
            Array.Copy(BitConverter.GetBytes(nodeId), 0, bytes, 8 + topics.Length, 8);//id
            Array.Copy(addr, 0, bytes, topics.Length + 16, addr.Length);//地址
            byte[] tmp = new byte[bytes.Length + 1];
            tmp[0] = 4;
            Array.Copy(bytes, 0, tmp, 1, bytes.Length);
            return tmp;
        }

        /// <summary>
        /// 触发更新数据列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, List<AddressInfo>> UnPackTriggerUpdatePublicList(byte[] data, int count,out long nodeid)
        {
            byte[] len = new byte[2];
            byte[] tmp = new byte[1024];
            Dictionary<string, List<AddressInfo>> dic = new Dictionary<string, List<AddressInfo>>();
            MemoryStream memory = new MemoryStream(data);
            memory.ReadByte();//去除标识

            List<AddressInfo> lst = null;
            byte[] bufID = new byte[8];
            memory.Read(bufID, 0, 8);
            nodeid = BitConverter.ToInt64(bufID, 0);
            while (memory.Position < count)
            {
                lst = new List<AddressInfo>();
                memory.Read(len, 0, 2);
                short infoLen = BitConverter.ToInt16(len, 0);
                memory.Read(len, 0, 2);
                short topicLen = BitConverter.ToInt16(len, 0);
                memory.Read(tmp, 0, topicLen);
                string topic = Encoding.Default.GetString(tmp, 0, topicLen);
                short addrLen = (short)(infoLen - topicLen - 2);
                if (addrLen > 1024)
                {
                    tmp = new byte[addrLen];
                }
                string str = null;
                memory.Read(tmp, 0, addrLen);
                str = Encoding.Default.GetString(tmp, 0, addrLen);
                string[] address = str.Split(';');
                foreach (var p in address)
                {
                    AddressInfo addr = new AddressInfo();
                    addr.Reset(p);
                    lst.Add(addr);
                }
                dic[topic] = lst;
            }
            memory.Close();
            return dic;
        }

        /// <summary>
        /// 触发更新数据列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] PackTriggerUpdatePublicList(long nodeid, Dictionary<string, List<AddressInfo>> pairs)
        {
            List<byte> lst = new List<byte>();
            lst.Add(10);
            lst.AddRange(BitConverter.GetBytes(nodeid));
            StringBuilder builder = new StringBuilder();
            foreach (var kv in pairs)
            {
                builder.Clear();
                byte[] topic = Encoding.Default.GetBytes(kv.Key);
                foreach (var addr in kv.Value)
                {
                    builder.AppendFormat("{0};", addr);
                }
                builder.Remove(builder.Length - 1, 1);
                byte[] tmp = Encoding.Default.GetBytes(builder.ToString());
                //
                lst.AddRange(BitConverter.GetBytes((short)(topic.Length + tmp.Length + 2)));
                lst.AddRange(BitConverter.GetBytes((short)(topic.Length)));
                lst.AddRange(topic);
                lst.AddRange(tmp);
            }
            return lst.ToArray();
        }

    }
}
