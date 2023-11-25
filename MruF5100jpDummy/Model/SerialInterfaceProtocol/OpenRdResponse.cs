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

        protected override byte[] CommandPayloadByteArray
        {
            get
            {
                List<byte> data = new List<byte>();

                data.Add((byte)(0x30 + YoukyuuOutouKekka));
                data.Add((byte)(0x30 + YoukyuuJuriNgSyousai)); // 仮固定
                data.AddRange(ByteArrayToAsciiArray(SplitIntInto2ByteDigitsArray(Id.Length)));
                data.AddRange(ConvertDigitsToAsciiArray(Id));

                return data.ToArray();
            }
        }

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
