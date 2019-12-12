using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ObserverDDS
{

    /// <summary>
    /// 处理TCP链接
    /// </summary>
    public class ClientProcess
    {
        private static Lazy<ClientProcess> Obj = new Lazy<ClientProcess>();

        ConcurrentDictionary<string, TopicTcp> dictionary = new ConcurrentDictionary<string, TopicTcp>();

        private int SocketNum = 0;

        private const int MaxNum = 20;

        private volatile bool IsRun = false;

        private int lastTime = 0;

        public static ClientProcess Instance
        {
            get { return Obj.Value; }
        }
       
        public ClientProcess()
        {
            StartThread();
        }

        private void StartThread()
        {
            Thread checkTcp = new Thread(() =>
              {
                  while (true)
                  {
                      Thread.Sleep(3000);
                      Check();
                  }
              });
            checkTcp.IsBackground = true;
            checkTcp.Name = "tcpclientcheck";
            if(!checkTcp.IsAlive)
            {
                checkTcp.Start();
            }
        }

        private void Check()
        {
            //
            if(IsRun)
            { 
                return;
            }
            IsRun = true;
            List<double> lst = new List<double>();
            List<string> lstTopic = new List<string>();
            Dictionary<double, string> dic = new Dictionary<double, string>();
            foreach(var kv in dictionary)
            {
                dic[kv.Value.Rate] = kv.Key;
                lstTopic.Add(kv.Key);

            }
            lst.AddRange(dic.Keys);
            lst.Sort();//排序
            List<string> lstKey = new List<string>();
            string key ;
            TopicTcp tcp = null;
            int num = MaxNum;
            for (int i = lst.Count - 1; i > 0; i--)
            {
                if(dic.TryGetValue(lst[i],out key))
                {
                   
                    if(dictionary.TryGetValue(key,out tcp))
                    {
                        //1秒内投递10次以上
                        if (tcp.Rate * 1000000 > 10)
                        {
                            num -= tcp.ClientSocket.Count;
                            lstKey.Add(key);
                            if (num < 0)
                            {
                                break;
                            }
                        }
                    }
                }
                //
            }
            foreach(string topic in lstTopic)
            {
                //关闭其它链接
                if(!lstKey.Contains(topic))
                {
                    if(dictionary.TryRemove(topic,out tcp))
                    {
                        //可能正在使用发送数据
                        lock (tcp.ClientSocket)
                        {
                            for (int i = 0; i < tcp.ClientSocket.Count; i++)
                            {
                                try
                                {
                                    Interlocked.Decrement(ref SocketNum);
                                    tcp.ClientSocket[i].Close();
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
            }
            IsRun = false;
        }
      
        /// <summary>
        /// 获取Topic的socket
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public  List<TcpClientSocket> GetTcpClients(string topic)
        {
            TopicTcp tcp = null;
            if(dictionary.TryGetValue(topic,out tcp))
            {
                return tcp.ClientSocket;
            }
            return null;
        }

        /// <summary>
        /// 更新socket
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="socket"></param>
        public void Update(string topic,TcpClientSocket socket)
        {
            TopicTcp tcp = new TopicTcp();
            tcp=  dictionary.GetOrAdd(topic, tcp);
            tcp.Refresh();
            if (tcp.Add(socket))
            {
                if (Interlocked.Increment(ref SocketNum) > MaxNum * 2)
                {
                    Task.Factory.StartNew(() =>
                    {
                        if (DateTime.Now.Second - lastTime > 3)
                        {
                            Check();
                        }
                    });
                }
            }
        }
    }
}
