using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sub
{
    class Program
    {
        static void Main(string[] args)
        {
            ObserverNet.NetSubscriber netSubscriber = new ObserverNet.NetSubscriber();
            netSubscriber.Subscribe("test");
            netSubscriber.CallBack += NetSubscriber_CallBack;
           
        }

        private static void NetSubscriber_CallBack(string topic, byte[] data)
        {
            Console.WriteLine(topic + ":" + Encoding.Default.GetString(data));
        }
    }
}
