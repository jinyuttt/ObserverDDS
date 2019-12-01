using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pub
{
    class Program
    {
        static void Main(string[] args)
        {
            ObserverDDS.NetPublisher publisher = new ObserverDDS.NetPublisher();
            string str = DateTime.Now.Second.ToString();
            while(true)
            {
                publisher.Publish("test", Encoding.Default.GetBytes(str+"_"+DateTime.Now.ToString()));
                Thread.Sleep(100);
            }
        }
    }
}
