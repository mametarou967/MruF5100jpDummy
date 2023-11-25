using MruF5100jpDummy.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public enum InventryMethod
    {
        [StringValue("ポーリング方式")]
        Polling = 0,
        [StringValue("レポート方式")]
        Report = 1,
        [StringValue("同期方式")]
        Synchronous = 2,
    }

    public enum DuplicateCheck
    {
        [StringValue("重複なし")]
        Absent=0,    // 重複が「なし」を表す
        [StringValue("重複あり")]
        Present=1,  // 重複が「あり」を表す
    }



    public class StartInvRequest : Command
    {
        public string Id { get; private set; }

        public override CommandType CommandType => CommandType.StartInv;

        public override DenbunType DenbunType => DenbunType.Request;

        public override byte Result => 0;

        protected override byte[] CommandPayloadByteArray
        {
            get
            {
                List<byte> data = new List<byte>();

                data.Add((byte)(((byte)DuplicateCheck << 7) | (byte)InventryMethod));

                return data.ToArray();
            }
        }

        protected override string CommadString => $"インベントリ方式:{InventryMethod.GetStringValue()} 重複チェック:{DuplicateCheck.GetStringValue()}";

        InventryMethod InventryMethod = InventryMethod.Polling;

        DuplicateCheck DuplicateCheck = DuplicateCheck.Absent;

        public StartInvRequest(){}
    }
}
