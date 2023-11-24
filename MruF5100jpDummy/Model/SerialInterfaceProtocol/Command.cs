using MruF5100jpDummy.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public enum CommandType
    {
        [StringValue("ダミー")]
        DummyCommand = 0,
        [StringValue("認証要求")]
        NinshouYoukyuu = 1,
        [StringValue("認証要求応答")]
        NinshouYoukyuuOutou = 2,
        [StringValue("認証状態要求")]
        NinshouJoutaiYoukyuu = 3,
        [StringValue("認証状態要求応答")]
        NinshouJoutaiYoukyuuOutou = 4
    }

    public enum NyuutaishitsuHoukou
    {
        [StringValue("入室")]
        Nyuushitsu = 1,
        [StringValue("退室")]
        Taishitsu = 2
    }

    public abstract class Command : ICommand
    {
        public int IdTanmatsuAddress { get; private set; }
        public NyuutaishitsuHoukou NyuutaishitsuHoukou { get; private set; }

        public Command(
            int idTanmatsuAddress,
            NyuutaishitsuHoukou nyuutaishitsuHoukou
           )
        {
            IdTanmatsuAddress = idTanmatsuAddress;
            NyuutaishitsuHoukou = nyuutaishitsuHoukou;
        }

        public abstract CommandType CommandType { get; }

        // public override abstract string ToString();

        public override string ToString() => BaseHeaderString + CommadString;

        protected abstract string CommadString { get; }

        private string BaseHeaderString =>
            $"ｺﾏﾝﾄﾞ:{CommandType.GetStringValue()} " +
            $"ID端末ｱﾄﾞﾚｽ:{IdTanmatsuAddress} " +
            $"入退室方向:{NyuutaishitsuHoukou.GetStringValue()} ";

        protected abstract byte[] CommandPayloadByteArray { get; }
  
        public byte[] ByteArray
        {
            get
            {
                List<byte> data_tmp1 = new List<byte>();

                data_tmp1.AddRange(ByteArrayToAsciiArray(IntTo2ByteArray(IdTanmatsuAddress)));
                data_tmp1.Add((byte)(0x30 + NyuutaishitsuHoukou));
                data_tmp1.AddRange(ByteArrayToAsciiArray(IntTo2ByteArray((int)CommandType)));
                data_tmp1.AddRange(CommandPayloadByteArray);

                List<byte> data_tmp2 = new List<byte>();

                data_tmp2.AddRange(IntTo2ByteArray(data_tmp1.Count));
                data_tmp2.AddRange(data_tmp1);
                data_tmp2.Add(0x03); // ETX

                List<byte> data = new List<byte>();

                data.Add(0x02); // STX
                data.AddRange(data_tmp2);
                data.Add(XorBytes(data_tmp2.ToArray())); // BCC

                return data.ToArray();

            }
        }

        protected byte[] IntTo2ByteArray(int value)
        {
            byte[] byteArray = new byte[2];
            byteArray[0] = (byte)((value >> 8) & 0xFF); // 上位バイト
            byteArray[1] = (byte)(value & 0xFF);        // 下位バイト
            return byteArray;
        }

        protected byte[] ByteArrayToAsciiArray(byte[] data) => data.Select(x => (byte)(x + 0x30)).ToArray();

        protected byte[] ConvertDigitsToAsciiArray(string input)
        {
            byte[] result = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    result[i] = (byte)(input[i]);
                }
                else
                {
                    throw new ArgumentException("Input string contains non-digit characters.");
                }
            }

            return result;
        }

        protected byte XorBytes(byte[] byteArray)
        {
            byte result = 0;
            foreach (byte b in byteArray)
            {
                result ^= b;
            }
            return result;
        }

    }
}
