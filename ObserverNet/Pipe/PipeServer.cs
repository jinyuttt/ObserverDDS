#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ObserverDDS
* 项目描述 ：
* 类 名 称 ：P
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
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace ObserverDDS
{
   public class PipeServer
    {
        public string RspMessage { get; set; }
        private volatile bool isRun = true;
        public void Start()
        {
            Thread srv = new Thread(new ParameterizedThreadStart((obj) =>
              {
                  using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("ObserverDDS", PipeDirection.InOut))
                  {
                      while (isRun)
                      {
                          pipeServer.WaitForConnection();

                        
                          StreamReader sr = new StreamReader(pipeServer);
                          sr.ReadToEnd();
                          using (StreamWriter sw = new StreamWriter(pipeServer))
                          {
                              sw.AutoFlush = true;
                              sw.WriteLine(RspMessage);
                              sw.Flush();
                          }
                      }
                  }
              }));
        }
    }
}
