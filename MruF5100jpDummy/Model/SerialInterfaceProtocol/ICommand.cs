using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public interface ICommand
    {
        CommandType CommandType { get; }

        byte[] ByteArray();

        string ToString();
    }
}
