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

        
        /// <summary>
        /// 使用频率
        /// </summary>
        public double Rate { get { return Num / (DateTime.Now.Ticks - Start); } }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh()
        {
            Interlocked.Increment(ref Num);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        internal bool Add(TcpClientSocket socket)
        {
            lock(ClientSocket)
            {
                if(ClientSocket.Contains(socket))
                {
                    socket.Close();
                    return false;
                }
                else
                {
                    ClientSocket.Add(socket);
                    return true;
                }
            }
        }
    }
}
