using MruF5100jpDummy.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public enum AntennaCh
    {
        [StringValue("アンテナ0")]
        ANT0 = 0,
        [StringValue("アンテナ1")]
        ANT1 = 1,
        [StringValue("アンテナ2")]
        ANT2 = 2,
        [StringValue("アンテナ3")]
        ANT3 = 3,
    }

    public class TagInfo
    {
        public AntennaCh AntennaCh { get; } = AntennaCh.ANT0;
        public int TimeStampMs { get; } = 10000;
        public ushort Repetition { get; } = 1;// 繰り返し検出回数
        public ushort Rssi { get; } = 10;
        public ushort Crc { get; } = 20;
        public ushort Pc { get; } = 30;
        public byte[] Epc { get; } = { 0x01, 0x02, 0x03 };

        public byte Len()
        {
            return (byte)(14 + Epc.Length);
        }

        public byte[] ByteArray()
        {
            List<byte> data = new List<byte>();
            data.Add(Len());
            data.Add((byte)AntennaCh);
            data.Add((byte)(TimeStampMs >> 24));
            data.Add((byte)(TimeStampMs >> 16));
            data.Add((byte)(TimeStampMs >> 8));
            data.Add((byte)TimeStampMs);
            data.Add((byte)(Repetition >> 8));
            data.Add((byte)Repetition);
            data.Add((byte)(Rssi >> 8));
            data.Add((byte)Rssi);
            data.Add((byte)(Crc >> 8));
            data.Add((byte)Crc);
            data.Add((byte)(Pc >> 8));
            data.Add((byte)Pc);
            data.AddRange(Epc);

            return data.ToArray();
        }

        public TagInfo(){}
    }
}
