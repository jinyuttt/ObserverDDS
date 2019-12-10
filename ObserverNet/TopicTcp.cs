using System;
using System.Collections.Generic;
using System.Threading;

namespace ObserverDDS
{
    public class TopicTcp
    {
        public List<TcpClientSocket> ClientSocket=new List<TcpClientSocket>();

        private int Num = 0;

        public long Start = DateTime.Now.Ticks;

        public double Rate { get { return Num / (DateTime.Now.Ticks - Start); } }

        public void Refresh()
        {
            Interlocked.Increment(ref Num);
        }

        internal void Add(TcpClientSocket socket)
        {
            lock(ClientSocket)
            {
                if(ClientSocket.Contains(socket))
                {
                    socket.Close();
                }
                else
                {
                    ClientSocket.Add(socket);
                }
            }
        }
    }
}
