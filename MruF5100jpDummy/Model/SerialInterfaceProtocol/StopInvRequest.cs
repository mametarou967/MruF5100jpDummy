using MruF5100jpDummy.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public class StopInvRequest : Command
    {
        public string Id { get; private set; }

        public override CommandType CommandType => CommandType.StopInv;

        public override DenbunType DenbunType => DenbunType.Request;

        public override byte Result => 0;

        protected override byte[] CommandPayloadByteArray => new byte[0];

        protected override string CommadString => "";

        public StopInvRequest(){}
    }
}
