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
            //ObserverDDS.NetSubscriber netSubscriber = new ObserverDDS.NetSubscriber();
            //netSubscriber.Subscribe("test");
            //netSubscriber.CallBack += NetSubscriber_CallBack;
            ObjectNetDDS.ObjectSubscriber subscriber = new ObjectNetDDS.ObjectSubscriber();
            subscriber.Subscribe<Model.Person>("test");
            subscriber.CallBack += Subscriber_CallBack;
            Console.Read();
        }

        private static void Subscriber_CallBack(string topic, object data)
        {
            Console.WriteLine(topic + ":" + data);
        }

        private static void NetSubscriber_CallBack(string topic, byte[] data)
        {
            Console.WriteLine(topic + ":" + Encoding.Default.GetString(data));
        }
    }
}
