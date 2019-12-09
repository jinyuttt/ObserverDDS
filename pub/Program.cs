using Model;
using System;
using System.Threading;

namespace pub
{
    class Program
    {
        static void Main(string[] args)
        {
           ObjectNetDDS.ObjectPublisher publisher = new ObjectNetDDS.ObjectPublisher();
            string str = DateTime.Now.Second.ToString();
            while (true)
            {
                Person person = new Person() { Age = 23, Name = "jason", Send = DateTime.Now.Second };
                publisher.Publish("test", person);
                Thread.Sleep(1000);
            }
        }
    }
}
