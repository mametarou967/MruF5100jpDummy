using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public class DummyCommand : Command
    {
        public override CommandType CommandType => CommandType.DummyCommand;

        public override DenbunType DenbunType => DenbunType.Request;

        public override byte Result => 0;

        protected override byte[] CommandPayloadByteArray => new byte[] { };

        protected override string CommadString => "";

        public DummyCommand()
        {

        }
    }
}
