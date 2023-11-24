﻿using System.Collections.Generic;
using System.IO.Ports;

namespace MruF5100jpDummy.Model.SerialPortManager
{
    static class SerialPortManager
    {
        static public List<string> GetAvailablePortNames()
        {
            string[] portNames = SerialPort.GetPortNames();
            var portList = new List<string>(portNames);
            portList.Reverse();
            return portList;
        }
    }
}
