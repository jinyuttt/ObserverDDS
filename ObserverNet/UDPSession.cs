using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
namespace ObserverNet
{
   public class UDPSession
    {
        private ConcurrentDictionary<string, UDPPackage> dic = new ConcurrentDictionary<string, UDPPackage>();
        public void AddData(string address, byte[] data)
        {
            UDPPackage uDP = new UDPPackage();
            var v = dic.GetOrAdd(address, uDP);
            v.AddData(data);
        }

    }
}
