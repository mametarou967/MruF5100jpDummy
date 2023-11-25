using MruF5100jpDummy.Model.Common;
using System.Collections.Generic;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{

    public enum YoukyuuOutouKekka
    {
        [StringValue("要求受理OK")]
        YoukyuuJuriOk = 0,
        [StringValue("要求受理NG")]
        YoukyuuJuriNg = 1
    }

    public enum YoukyuuJuriNgSyousai
    {
        [StringValue("なし")]
        Nashi = 0,
    }

    public class OpenRdResponse : Command
    {
        public YoukyuuOutouKekka YoukyuuOutouKekka { get; private set; }
        public YoukyuuJuriNgSyousai YoukyuuJuriNgSyousai { get; private set; }
        public string Id { get; private set; }

        protected override byte[] CommandPayloadByteArray => new byte[0];

        public override CommandType CommandType => CommandType.OpenRd;

        public override DenbunType DenbunType => DenbunType.Response;

        public override int dataSize => 0;

        public override byte Result => 0;

        protected override string CommadString => "";

        public OpenRdResponse
        () 
        {        }
    }
}
