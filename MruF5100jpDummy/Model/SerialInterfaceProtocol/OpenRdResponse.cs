﻿using MruF5100jpDummy.Model.Common;
using System.Collections.Generic;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public class OpenRdResponse : Command
    {
        public string Id { get; private set; }

        protected override byte[] CommandPayloadByteArray => new byte[0];

        public override CommandType CommandType => CommandType.OpenRd;

        public override DenbunType DenbunType => DenbunType.Response;

        public override int dataSize => 0;

        public override byte Result => 0;

        protected override string CommadString => "";

        public OpenRdResponse(){}
    }
}
