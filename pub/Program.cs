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
            ObserverNet.NetPublisher publisher = new ObserverNet.NetPublisher();
            while(true)
            {
                publisher.Publish("test", Encoding.Default.GetBytes(DateTime.Now.ToString()));
                Thread.Sleep(1000);
            }
        }
    }
}
