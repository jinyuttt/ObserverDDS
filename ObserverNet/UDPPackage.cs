using System;
using System.Collections.Generic;
using System.Text;

namespace ObserverNet
{
   public class UDPPackage
    {
        SessionPackage[] sessions = null;
        public void AddData(byte[]data)
        {

        }
        public void AddData(SessionPackage[] session)
        {
            sessions = session;
        }
    }
}
