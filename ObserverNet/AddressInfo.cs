#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：AddressInfo
* 类 描 述 ：
* 所在的域 ：DESKTOP-1IBOINI
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
using System.Collections.Generic;
using System.Text;

namespace ObserverDDS
{

    /// <summary>
    /// 地址信息
    /// </summary>
  public  class AddressInfo
    {
        /// <summary>
        /// IP
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 0=tcp,1=udp
        /// </summary>
        public int Protol { get; set; }

        public override string ToString()
        {
            return String.Format("{0}_{1}_{2}", Protol, Address, Port);
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="addr"></param>
        public void Reset(string addr)
        {
            string[] tmp = addr.Split('_');
            Address = tmp[1];
            Port =int.Parse(tmp[2]);
            Protol = int.Parse(tmp[0]);
        }

        public override bool Equals(object obj)
        {
            AddressInfo tmp = obj as AddressInfo;
            if(tmp==null)
            {
                return false;
            }
            else if(this.ToString()==obj.ToString())
            {
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
