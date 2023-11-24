﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public class NinshouYoukyuuCommand : Command
    {
        public string Id { get; private set; }

        public override CommandType CommandType => CommandType.NinshouYoukyuu;

        protected override byte[] CommandPayloadByteArray
        {
            get
            {
                List<byte> data = new List<byte>();

                data.AddRange(ByteArrayToAsciiArray(IntTo2ByteArray(Id.Length)));
                data.AddRange(ConvertDigitsToAsciiArray(Id));

                return data.ToArray();
            }
        }

        protected override string CommadString => $"利用者ID:{Id}";

        public NinshouYoukyuuCommand(
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou,
            string id
        ) : base(idTanmatsuAddress, nyuutaishitsuHoukou)
        {
            Id = id;
        }
    }
}
