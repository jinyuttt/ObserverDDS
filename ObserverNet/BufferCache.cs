#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：BufferCache
* 类 描 述 ：
* 命名空间 ：ObserverDDS
* 机器名称 ：DESKTOP-1IBOINI
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu  
* 创建时间 ：2019
* 更新时间 ：2019
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2019. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion




using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ObserverDDS
{

    /// <summary>
    /// 获取缓存
    /// </summary>
    public class BufferCache
    {
        private static readonly Lazy<BufferCache> obj = new Lazy<BufferCache>();

        private readonly ConcurrentQueue<RecviceBuffer> buffers = new ConcurrentQueue<RecviceBuffer>();

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
            get { return obj.Value; }
        }

        public BufferCache()
        {
            if (IsCache)
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
            if (!buffers.TryDequeue(out buffer))
            {
                if (Interlocked.Decrement(ref Num) > 0)
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
