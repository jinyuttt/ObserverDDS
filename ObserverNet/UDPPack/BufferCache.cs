using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ObserverDDS
{

    /// <summary>
    /// 获取缓存
    /// </summary>
    public  class BufferCache
    {
        private static Lazy<BufferCache> obj = new Lazy<BufferCache>();

        private ConcurrentQueue<RecviceBuffer> buffers = new ConcurrentQueue<RecviceBuffer>();

        private int Num = 0;

        /// <summary>
        /// 是否采用缓存
        /// </summary>
        public static bool IsCache = true;

        /// <summary>
        /// 缓存大小M
        /// 默认80M
        /// </summary>
        private const int CacheSzie = 80;


        public static BufferCache Instance
        {
            get {return obj.Value; }
        }

        public BufferCache()
        {
            if(IsCache)
            {
                Num = CacheSzie * 1024 * 1024 / UDPPack.PackSize;
            }
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="recviceBuffer"></param>
        internal void Add(RecviceBuffer recviceBuffer)
        {
            buffers.Enqueue(recviceBuffer);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <returns></returns>
        public RecviceBuffer GetBuffer()
        {
            RecviceBuffer buffer = null;
            if(!buffers.TryDequeue(out buffer))
            {
                if(Interlocked.Decrement(ref Num)>0)
                {
                    buffer = new RecviceBuffer(this)
                    {
                        Data = new byte[UDPPack.PackSize]
                    };
                }
                else
                {
                    buffer = new RecviceBuffer
                    {
                        Data = new byte[UDPPack.PackSize]
                    };
                }
            }
            return buffer;
        }
    }
}
