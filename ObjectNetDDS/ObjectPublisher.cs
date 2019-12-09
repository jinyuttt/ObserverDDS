#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObjectDDS
* 项目描述 ：
* 类 名 称 ：DDSExpansion
* 类 描 述 ：
* 命名空间 ：ObjectDDS
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




using ObserverDDS;

namespace ObjectNetDDS
{

    public  class ObjectPublisher
    {
        private readonly NetPublisher publisher = new NetPublisher();
         
        
        /// <summary>
        /// 发布数据，二进制序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topic"></param>
        /// <param name="obj"></param>
        public   void Publish<T>(string topic, T obj)
        {
           var data= MsgSerializer.Serializer.Serialize(obj);
            publisher.Publish(topic, data);
        }

       /// <summary>
       /// 发布数据，JSON格式序列化，UTF8编码字节数组
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="topic"></param>
       /// <param name="obj"></param>
        public void PublishJson<T>(string topic, T obj)
        {
            var data = MsgSerializer.Serializer.JSONSerialize(obj);
            publisher.Publish(topic, data);
        }
    }
}
