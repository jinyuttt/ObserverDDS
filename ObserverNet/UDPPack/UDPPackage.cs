using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ObserverDDS
{

    /// <summary>
    /// 一个大数据包
    /// </summary>
   public class UDPPackage
    {
        SubPackage[] sessions = null;
        public  int SessionId { get; set; }

        public IPEndPoint RemoteHost { get; set; }

        public SubPackage[] Packages { get { return sessions; } }

        public  int MaxNum { get; set; }

        public  int CreateTime { get; set; }

        private volatile int count = 0;

        public void AddData(byte[]data,int len)
        {
             var  p=   UDPPack.UnPack(data,len);
            if (sessions == null)
            {
                sessions = new SubPackage[p.PackNum];
            }
            sessions[p.SeqId] = p;
        }

        public bool AddData(SubPackage package)
        {
          
            if (sessions == null)
            {
                sessions = new SubPackage[package.PackNum];
            }
            sessions[package.SeqId] = package;
            count++;
            if (count == sessions.Length)
            {
                
                return Check();
            }
            return false;
        }
        private   bool Check()
        {
            for(int i=0;i<sessions.Length;i++)
            {
                if(sessions[i]==null)
                {
                    count--;
                    return false;
                }
            }
            return true;
        }

        public void AddData(SubPackage[] session)
        {
            sessions = session;
        }
      
         public void Remove(int seqid)
        {
            sessions[seqid] = null;
        }
    }
}
