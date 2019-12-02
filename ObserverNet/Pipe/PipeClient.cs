#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：PipeClient
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



using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace ObserverDDS
{
    public class PipeClient
    {
        public void Connect()
        {
            var pipeClient = new NamedPipeClientStream(".", "ObserverDDS", PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
            pipeClient.Connect(5000);
            var swClient = new StreamWriter(pipeClient);
            var srClient = new StreamReader(pipeClient);
            swClient.AutoFlush = true;
        }
    }
}
