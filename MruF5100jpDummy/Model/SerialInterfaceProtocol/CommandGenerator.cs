using MruF5100jpDummy.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public enum ByteCheckResult
    {

        [StringValue("OK")]
        Ok,
        [StringValue("データなし")]
        NgNoByte,
        [StringValue("messageの長さフィールドを持っていない(3バイトより短い)")]
        NgHasNoLengthField,
        [StringValue("長さフィールド分のメッセージを持っていない")]
        NgMessageIncompleted,
        [StringValue("CRCエラー")]
        NgCrcError,
    }


    public static class CommandGenerator
    {

        public static ByteCheckResult ByteCheck(byte[] data)
        {
            if (data.Length == 0) return ByteCheckResult.NgNoByte;

            if (data.Length < 6) return ByteCheckResult.NgHasNoLengthField;

            var messageLength = (data[5] << 8) | data[4];

            if (data.Length < 16 + messageLength + 2) return ByteCheckResult.NgMessageIncompleted;

            ushort receiveCrc = (ushort)(data[16 + messageLength] + (data[16 + messageLength + 1] << 8));

            byte[] crcTarget = new byte[16 + messageLength];

            Array.Copy(data, 0, crcTarget, 0, 16 + messageLength);

            var calcCrc = Common.Common.CalculateCrc(crcTarget);

            if (receiveCrc != calcCrc) return ByteCheckResult.NgCrcError;

            return ByteCheckResult.Ok;
        }

        public static int? GetCommandByteLength(byte[] data)
        {
            if (data.Length < 6) return null;

            var messageLength = (data[5] << 8) | data[4];

            if (data.Length < 16 + messageLength + 2) return null;

            return 16 + messageLength + 2;
        }

        public static Command CommandGenerate(byte[] data)
        {
            // フォーマットはあっている前提
            var idTanmatsuAddress = ((data[3] - 0x30) * 10) + (data[4] - 0x30);

            var commandType = ((data[6] - 0x30) * 10) + (data[7] - 0x30);
            var nyuutaishitsuHoukou = (NyuutaishitsuHoukou)data[5] - 0x30;

            if (commandType == (int)CommandType.OpenRd)
            {
                var idLength = ((data[8] - 0x30) * 10) + (data[9] - 0x30);

                return new OpenRdRequest();
            }

            return new DummyCommand();
        }

        public static OpenRdResponse ResponseGenerate(
            OpenRdRequest ninshouYoukyuuCommand,
            YoukyuuOutouKekka youkyuuOutouKekka,
            YoukyuuJuriNgSyousai youkyuuJuriNgSyousai,
             bool idtAdrError = false,
             bool inoutDirError = false,
             bool riyoushaIdError = false,
             bool bccError = false
        )
        {
            return new OpenRdResponse();
        }

        static string ExtractAndConvert(byte[] byteArray, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex >= byteArray.Length || length <= 0)
            {
                return string.Empty; // 無効な引数の場合は空文字列を返す
            }

            int endIndex = startIndex + length - 1;
            endIndex = Math.Min(endIndex, byteArray.Length - 1);

            byte[] convertedChars = new byte[endIndex - startIndex + 1];

            for (int i = startIndex; i <= endIndex; i++)
            {
                convertedChars[i - startIndex] = byteArray[i];
            }

            var str = System.Text.Encoding.GetEncoding("shift_jis").GetString(convertedChars);
            return str;
        }

        static private int FixIdTanmatsuAddress(int idTanmatsuAddress, bool idtAdrError) =>
            (idtAdrError) ? idTanmatsuAddress + 1 : idTanmatsuAddress;

        static private NyuutaishitsuHoukou FixNyuutaishitsuHoukou(NyuutaishitsuHoukou nyuutaishitsuHoukou, bool inoutDirError)
        {
            NyuutaishitsuHoukou fixNyuutaishitsuHoukou = nyuutaishitsuHoukou;

            if (inoutDirError)
            {
                if (fixNyuutaishitsuHoukou == NyuutaishitsuHoukou.Nyuushitsu) fixNyuutaishitsuHoukou = NyuutaishitsuHoukou.Taishitsu;
                else fixNyuutaishitsuHoukou = NyuutaishitsuHoukou.Nyuushitsu;
            }

            return fixNyuutaishitsuHoukou;
        }

        static private string FixRiyoushaId(string riyoushaId, bool riyoushaIdError)
        {
            // 文字列を整数に変換して1増やす
            long riyoushaNumber = long.Parse(riyoushaId);
            if (riyoushaIdError) riyoushaNumber++;

            // 数値を文字列に戻す時、桁数を保持するためにPadLeftを使用
            return riyoushaNumber.ToString().PadLeft(riyoushaId.Length, '0');

        }
    }
}
