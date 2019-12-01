namespace ObserverDDS
{

    /// <summary>
    /// 数据小包
    /// </summary>
    public class SubPackage
    {
        public byte[] Data { get; set; }

        public int OffSet { get; set; }

        public int Len { get; set; }

        public int SeqId { get; set; }

        public int SessionId { get; set; }

        public int PackNum { get; set; }

        public byte DataType { get; set; }
    }
}
