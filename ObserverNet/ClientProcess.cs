using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ObserverDDS
{
    public class ClientProcess
    {
        ConcurrentDictionary<string, TopicTcp> dictionary = new ConcurrentDictionary<string, TopicTcp>();

        private int SocketNum = 0;

        private const int MaxNum = 20;

        private void Check()
        {
            //
            List<double> lst = new List<double>();
            Dictionary<double, string> dic = new Dictionary<double, string>();
            foreach(var kv in dictionary)
            {
                dic[kv.Value.Rate] = kv.Key;

            }
            lst.AddRange(dic.Keys);
            lst.Sort();//排序
            List<string> lstKey = new List<string>();
            string key ;
            for (int i = lst.Count - 1; i > 0; i--)
            {
                if(dic.TryGetValue(lst[i],out key))
                {

                }
            }
            
        }
        public  List<TcpClientSocket> GetTcpClients(string topic)
        {
            TopicTcp tcp = null;
            if(dictionary.TryGetValue(topic,out tcp))
            {
                return tcp.ClientSocket;
            }
            return null;
        }

        public void Update(string topic,TcpClientSocket socket)
        {
            TopicTcp tcp = new TopicTcp();
            tcp=  dictionary.GetOrAdd(topic, tcp);
            tcp.Refresh();
            tcp.Add(socket);
            if(Interlocked.Increment(ref SocketNum)>20)
            {

            }
        }
    }
}
