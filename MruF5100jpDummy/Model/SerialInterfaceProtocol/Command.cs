using MruF5100jpDummy.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public enum CommandType
    {
        [StringValue("ダミー")]
        DummyCommand = 99,
        [StringValue("OpenRdコマンド")]
        OpenRd = 0x00,
    }

    public enum DenbunType
    {
        [StringValue("リクエスト")]
        Request = 0x50,
        [StringValue("レスポンス")]
        Response = 0x51,
        [StringValue("レポート")]
        Report = 0x52,
        [StringValue("特殊")]
        Special = 0x53,
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
        public bool BccError { get; private set; }

        public int IdTanmatsuAddress { get; private set; }
        public NyuutaishitsuHoukou NyuutaishitsuHoukou { get; private set; }

        public Command()
        {
        }

        public abstract CommandType CommandType { get; }

        public abstract DenbunType DenbunType { get; }

        public abstract int dataSize { get; }

        public abstract byte Result { get; }

        // public override abstract string ToString();

        public override string ToString() => BaseHeaderString + CommadString + BaseFooterString;

        protected abstract string CommadString { get; }

        private string BaseHeaderString =>
            Common.Common.PaddingInBytes($"CMD: {CommandType.GetStringValue()} {DenbunType.GetStringValue()}", PadType.Char, 36);

        private string BaseFooterString => "";


        protected abstract byte[] CommandPayloadByteArray { get; }

        public byte[] ByteArray()
        {
            List<byte> data_tmp1 = new List<byte>();
            var dataSizeLower = (byte)(dataSize % 256);
            var dataSizeUpper = (byte)(dataSize / 256);
            data_tmp1.Add(0x00);                 // 電文のバージョン番号 (0x00)固定
            data_tmp1.Add((byte)DenbunType);     // 電文識別子 0x50:リクエスト 0x51:レスポンス
            data_tmp1.Add(0x00);                 // ID (0x00)固定
            data_tmp1.Add((byte)CommandType);    // コマンド番号
            data_tmp1.Add(dataSizeLower);        // データサイズ下位（リトルエンディアン）
            data_tmp1.Add(dataSizeUpper);        // データサイズ上位
            data_tmp1.Add(Result);               // コマンド処理結果
            data_tmp1.Add(0x00);                  // エラーコード いったん0
            data_tmp1.Add(0x00);                  // エラーコード いったん0
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.Add(0x00);                  // Reserve
            data_tmp1.AddRange(CommandPayloadByteArray);

            var crc = Common.Common.CalculateCrc(data_tmp1.ToArray());
            List<byte> data = new List<byte>();
            var crcLower = (byte)(crc % 256);
            var crcUpper = (byte)(crc / 256);
            data.AddRange(data_tmp1);
            data.Add(crcLower);
            data.Add(crcUpper);

            return data.ToArray();
        }

        protected byte[] IntTo2ByteArray(int value)
        {
            byte[] byteArray = new byte[2];
            byteArray[0] = (byte)((value >> 8) & 0xFF); // 上位バイト
            byteArray[1] = (byte)(value & 0xFF);        // 下位バイト
            return byteArray;
        }

        protected byte[] SplitIntInto2ByteDigitsArray(int value)
        {
            byte[] byteArray = new byte[2];
            byteArray[0] = (byte)(value / 10); // 上位バイト
            byteArray[1] = (byte)(value % 10); // 下位バイト
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
