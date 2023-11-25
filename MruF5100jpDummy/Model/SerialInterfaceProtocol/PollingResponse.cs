using MruF5100jpDummy.Model.Common;
using System.Collections.Generic;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public class PollingResponse : Command
    {
        public string Id { get; private set; }

        public override CommandType CommandType => CommandType.Polling;

        public override DenbunType DenbunType => DenbunType.Response;

        public override byte Result => 0;

        protected override string CommadString => $"タグ個数:{TagInfos.Count}個";

        public List<TagInfo> TagInfos { get; } = new List<TagInfo>() { new TagInfo() };

        protected override byte[] CommandPayloadByteArray
        {
            get
            {
                List<byte> data = new List<byte>();

                data.Add((byte)TagInfos.Count);
                TagInfos.ForEach(tagInfo => data.AddRange(tagInfo.ByteArray()));

                return data.ToArray();
            }
        }

        public PollingResponse(){}
    }
}
