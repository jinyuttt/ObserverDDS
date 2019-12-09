#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：pub
* 项目描述 ：
* 类 名 称 ：Person
* 类 描 述 ：
* 命名空间 ：pub
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
 public   class Person
    {
       public string Name { get; set; }

       public int Age { get; set; }

        public int Send { get; set; }

        public override string ToString()
        {
            return Name + "_" + Send;
        }
    }
}
