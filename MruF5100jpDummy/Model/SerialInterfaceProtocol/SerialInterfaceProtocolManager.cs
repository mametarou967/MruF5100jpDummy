using MruF5100jpDummy.Model.Common;
using MruF5100jpDummy.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialInterfaceProtocol
{
    public class SerialInterfaceProtocolManager
    {
        MruF5100jpDummy.Model.SerialCom.SerialCom serialCom;
        Queue<byte> receiveDataQueue = new Queue<byte>();
        ILogWriteRequester logWriteRequester;
        private readonly object sendLock = new object(); // ロックオブジェクト


        // 要求応答プロパティ
        public bool IsResponseEnableYoukyuuOutou = true;
        public bool IsIdtAdrErrorYoukyuuOutou = false;
        public bool IsInoutDirErrorYoukyuuOutou = false;
        public bool IsRiyoushaIdErrorYoukyuuOutou = false;
        public bool IsBccErrorYoukyuuOutou = false;
        public uint YoukyuuOutouJikanMs = 200;
        public YoukyuuOutouKekka YoukyuuOutouKekka = YoukyuuOutouKekka.YoukyuuJuriOk;

        // 要求状態応答プロパティ
        public bool IsResponseEnableYoukyuuJoutaiOutou = true;
        public bool IsIdtAdrErrorYoukyuuJoutaiOutou = false;
        public bool IsInoutDirErrorYoukyuuJoutaiOutou = false;
        public bool IsRiyoushaIdErrorYoukyuuJoutaiOutou = false;
        public bool IsBccErrorYoukyuuJoutaiOutou = false;
        public uint YoukyuuJoutaiOutouJikanMs = 200;
        public string RiyoushaId = "00043130";

        public SerialInterfaceProtocolManager(ILogWriteRequester logWriteRequester)
        {
            this.logWriteRequester = logWriteRequester;
        }

        public void ComStart(string comPort)
        {
            if ((serialCom != null) && serialCom.IsCommunicating)
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信中のため、新しい通信は開始しません");
            }
            else
            {
                serialCom = new SerialCom.SerialCom(comPort, DataReceiveAction, logWriteRequester);
                serialCom.StartCom();
            }
        }

        public void ComStop()
        {
            if ((serialCom == null) || !(serialCom.IsCommunicating))
            {
                logWriteRequester.WriteRequest(LogLevel.Error, "通信が開始されていないため、通信の停止処理は行いません");
            }
            else
            {
                serialCom?.StopCom();
            }
        }

        public void Send(ICommand command)
        {
            lock (sendLock) // ロックを獲得
            {

                try
                {
                    var byteArray = command.ByteArray();

                    if (serialCom != null)
                    {
                        logWriteRequester.WriteRequest(LogLevel.Info, "[送信] " + command.ToString());

                        // if (bccError) logWriteRequester.WriteRequest(LogLevel.Warning, "<i> bccError設定が有効のため、BCCエラーで送信します");

                        serialCom.Send(byteArray);
                    }
                }
                catch
               (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private void DataReceiveAction(byte[] datas)
        {
            // キュー詰め
            datas.ToList().ForEach(receiveDataQueue.Enqueue);


            while (receiveDataQueue.ToArray().Length != 0)
            {
                // 受信データの評価
                var byteCheckResult = CommandGenerator.ByteCheck(receiveDataQueue.ToArray());

                if (byteCheckResult == ByteCheckResult.Ok)
                {
                    // サイズを調べる
                    var size = CommandGenerator.GetCommandByteLength(receiveDataQueue.ToArray());

                    if (!size.HasValue) continue;

                    List<byte> commandBytes = new List<byte>();

                    // サイズ分デキューする
                    for (int i = 0; i < size.Value; i++)
                    {
                        commandBytes.Add(receiveDataQueue.Dequeue());
                    }

                    // バイト列から受信コマンドを生成する
                    var receiveCommand = CommandGenerator.CommandGenerate(commandBytes.ToArray());

                    logWriteRequester.WriteRequest(LogLevel.Info, "[受信] " + receiveCommand.ToString());

                    if (receiveCommand.DenbunType == DenbunType.Request)
                    {
                        var responseCommand = ResponseGenerate(receiveCommand);

                        if (responseCommand.CommandType != CommandType.DummyCommand)
                        {
                            // 有効な応答コマンドが生成されているので任意の時間経過後応答する
                            Task.Run(async () =>
                            {
                                Send(responseCommand);
                            });
                        }
                    }

                }
                else if (
                    (byteCheckResult == ByteCheckResult.NgNoByte) ||
                    (byteCheckResult == ByteCheckResult.NgHasNoLengthField) ||
                    (byteCheckResult == ByteCheckResult.NgMessageIncompleted))
                {
                    // データがたまるまで待つ
                    break;
                }
                else if (
                    (byteCheckResult == ByteCheckResult.NgCrcError))
                {
                    // サイズを調べる
                    var size = CommandGenerator.GetCommandByteLength(receiveDataQueue.ToArray());

                    if (!size.HasValue) continue;

                    // サイズ分デキューする
                    for (int i = 0; i < size.Value; i++)
                    {
                        receiveDataQueue.Dequeue();
                    }

                    logWriteRequester.WriteRequest(LogLevel.Error, $"{ byteCheckResult.GetStringValue()} のためメッセージを破棄します");
                }
            }
        }

        ICommand ResponseGenerate(
            ICommand command)
        {
            if (command.CommandType == CommandType.OpenRd)
            {
                // 受信コマンドの応答を生成
                var ninshouYoukyuuOutouCommand = CommandGenerator.ResponseGenerate(
                    command as OpenRdRequest,
                    YoukyuuOutouKekka,
                    YoukyuuJuriNgSyousai.Nashi // 一旦固定
                    );

                return ninshouYoukyuuOutouCommand;
            }

            return new DummyCommand();
        }
    }
}
