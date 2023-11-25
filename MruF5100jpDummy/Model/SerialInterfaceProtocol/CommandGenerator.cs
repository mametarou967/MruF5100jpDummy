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
            byte commandType = data[3];
            byte denbunType = data[1];

            if (commandType == (byte)CommandType.OpenRd)
            {
                if (denbunType == (byte)DenbunType.Request)
                {
                    return new OpenRdRequest();
                }
                else if (denbunType == (byte)DenbunType.Response)
                {
                    return new OpenRdResponse();
                }
            }
            else if (commandType == (byte)CommandType.CloseRd)
            {
                if (denbunType == (byte)DenbunType.Request)
                {
                    return new CloseRdRequest();
                }
                else if (denbunType == (byte)DenbunType.Response)
                {
                    return new CloseRdResponse();
                }
            }
            else if (commandType == (byte)CommandType.StartInv)
            {
                if (denbunType == (byte)DenbunType.Request)
                {
                    return new StartInvRequest();
                }
                else if (denbunType == (byte)DenbunType.Response)
                {
                    return new StartInvResponse();
                }
            }
            else if (commandType == (byte)CommandType.StopInv)
            {
                if (denbunType == (byte)DenbunType.Request)
                {
                    return new StopInvRequest();
                }
                else if (denbunType == (byte)DenbunType.Response)
                {
                    return new StopInvResponse();
                }
            }
            else if (commandType == (byte)CommandType.Polling)
            {
                if (denbunType == (byte)DenbunType.Request)
                {
                    return new PollingRequest();
                }
                else if (denbunType == (byte)DenbunType.Response)
                {
                    return new PollingResponse();
                }
            }

            return new DummyCommand();
        }

        public static OpenRdResponse ResponseGenerate(
            OpenRdRequest openRdRequest
        )
        {
            return new OpenRdResponse();
        }

        public static CloseRdResponse ResponseGenerate(
            CloseRdRequest closeRdRequest
        )
        {
            return new CloseRdResponse();
        }

        public static StartInvResponse ResponseGenerate(
            StartInvRequest startInvRequest
        )
        {
            return new StartInvResponse();
        }

        public static StopInvResponse ResponseGenerate(
            StopInvRequest startInvRequest
        )
        {
            return new StopInvResponse();
        }

        public static PollingResponse ResponseGenerate(
            PollingRequest startInvRequest
        )
        {
            return new PollingResponse();
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
