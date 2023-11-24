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
        public uint YoukyuuOutouJikanMs = 1000;
        public YoukyuuOutouKekka YoukyuuOutouKekka = YoukyuuOutouKekka.YoukyuuJuriOk;
        
        // 要求状態応答プロパティ
        public uint YoukyuuJoutaiOutouJikanMs = 1000;
        public NinshouJoutai NinshouJoutai = NinshouJoutai.Syorinashi;
        public NinshouKanryouJoutai NinshouKanryouJoutai = NinshouKanryouJoutai.NinshouKekkaNashi;
        public NinshouKekkaNgShousai NinshouKekkaNgShousai = NinshouKekkaNgShousai.Nashi;
        public string RiyoushaId = "00043130";

        public void ComStart(

            string comPort,
            ILogWriteRequester logWriteRequester)
        {
            this.logWriteRequester = logWriteRequester;
            serialCom = new SerialCom.SerialCom(comPort, DataReceiveAction, logWriteRequester);
            serialCom.StartCom();
        }

        public void ComStop()
        {
            serialCom?.StopCom();
        }

        public void Send(ICommand command)
        {
            lock (sendLock) // ロックを獲得
            {

                try
                {
                    var byteArray = command.ByteArray;

                    if (serialCom != null)
                    {
                        logWriteRequester.WriteRequest("[送信] " + command.ToString());

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


            while(receiveDataQueue.ToArray().Length != 0)
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

                    logWriteRequester.WriteRequest("[受信] " + receiveCommand.ToString());

                    var responseCommand = ResponseGenerate(receiveCommand);

                    if(responseCommand.CommandType != CommandType.DummyCommand)
                    {
                        // 有効な応答コマンドが生成されているので任意の時間経過後応答する
                        Task.Run(async () =>
                        {
                            uint outouJikanMs = 1000;

                            if (responseCommand.CommandType == CommandType.NinshouYoukyuuOutou) outouJikanMs = YoukyuuOutouJikanMs;
                            else if (responseCommand.CommandType == CommandType.NinshouJoutaiYoukyuuOutou) outouJikanMs = YoukyuuJoutaiOutouJikanMs;

                            await Task.Delay(TimeSpan.FromMilliseconds(outouJikanMs));

                            Send(responseCommand);
                        });
                    }

                }
                else if (byteCheckResult == ByteCheckResult.NgNoStx)
                {
                    // 先頭をdequeueして終了
                    receiveDataQueue.Dequeue();

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
                    (byteCheckResult == ByteCheckResult.NgNoEtx) ||
                    (byteCheckResult == ByteCheckResult.NgBccError))
                {
                    // サイズを調べる
                    var size = CommandGenerator.GetCommandByteLength(receiveDataQueue.ToArray());

                    if (!size.HasValue) continue;

                    List<byte> commandBytes = new List<byte>();

                    // サイズ分デキューする
                    for (int i = 0; i < size.Value; i++)
                    {
                        receiveDataQueue.Dequeue();
                    }
                }
            }
        }

        ICommand ResponseGenerate(ICommand command)
        {
            if (command.CommandType == CommandType.NinshouYoukyuu)
            {
                // 受信コマンドの応答を生成
                var ninshouYoukyuuOutouCommand = CommandGenerator.ResponseGenerate(
                    command as NinshouYoukyuuCommand,
                    YoukyuuOutouKekka,
                    YoukyuuJuriNgSyousai.Nashi // 一旦固定
                    );

                return ninshouYoukyuuOutouCommand;
            }
            else if (command.CommandType == CommandType.NinshouJoutaiYoukyuu)
            {
                // 受信コマンドの応答を生成
                var ninshouJoutaiYoukyuuOutouCommand = CommandGenerator.ResponseGenerate(
                    command as NinshouJoutaiYoukyuuCommand,
                    NinshouJoutai,
                    NinshouKanryouJoutai,
                    NinshouKekkaNgShousai,
                    RiyoushaId
                    );

                return ninshouJoutaiYoukyuuOutouCommand;
            }

            return new DummyCommand(0, NyuutaishitsuHoukou.Nyuushitsu);
        }
    }
}
