using MruF5100jpDummy.Model.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MruF5100jpDummy.Model.SerialCom
{
    public class SerialCom
    {
        private string comport;
        Task serialComTask;
        private CancellationTokenSource cancellationTokenSource;
        private SerialPort serialPort;
        ConcurrentQueue<byte> sendQueue = new ConcurrentQueue<byte>();
        ILogWriteRequester logWriteRequester;
        Action<byte[]> dataReceiveAction;

        public SerialCom(
            string comport,
            Action<byte[]> dataReceiveAction,
            ILogWriteRequester logWriteRequester)
        {
            this.comport = comport;
            this.dataReceiveAction = dataReceiveAction;
            this.logWriteRequester = logWriteRequester;
        }

        public bool StartCom()
        {
            if (serialPort != null || cancellationTokenSource != null) return false;

            serialPort = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One);
            serialPort.Open();

            logWriteRequester.WriteRequest(
                $"[COM-START1] " +
                $"PortName:{serialPort.PortName} " +
                $"OpenStatus:{serialPort.IsOpen} ");

            logWriteRequester.WriteRequest(
                $"[COM-START2] " +
                $"DataBits:{serialPort.DataBits} " +
                $"BaudRate:{serialPort.BaudRate} " +
                $"Parity:{serialPort.Parity} " +
                $"StopBits:{serialPort.StopBits}" );

            cancellationTokenSource = new CancellationTokenSource();

            serialComTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (serialPort.BytesToRead > 0)
                    {
                        byte[] receivedBytes = new byte[serialPort.BytesToRead];
                        serialPort.Read(receivedBytes, 0, receivedBytes.Length);

                        // 受信データバイナリログ出力
                        logWriteRequester.WriteRequest("[RCV] " + string.Join(" ", receivedBytes.Select(b => $"0x{b:X2}")));

                        dataReceiveAction?.Invoke(receivedBytes);
                    }

                    var sendList = new List<byte>();
                    // sendQueueを調べてデータがあればserialPortに送信する
                    while (!sendQueue.IsEmpty)
                    {
                        if(sendQueue.TryDequeue(out byte data))
                        {
                            sendList.Add(data);
                        }
                    }

                    if (sendList.Count != 0)
                    {
                        serialPort.Write(sendList.ToArray(), 0, sendList.Count);

                        // 送信データバイナリログ出力
                        logWriteRequester.WriteRequest("[SND] " + string.Join(" ", sendList.ToArray().Select(b => $"0x{b:X2}")));

                    }

                    await Task.Delay(100); // Adjust the delay duration as needed
                }
            });

            return true;
        }

        public bool StopCom()
        {
            bool result = true;

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                try
                {
                    var waitResult = serialComTask.Wait(5000);
                    if (!waitResult) result = false;
                }
                catch
                {
                }
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                logWriteRequester.WriteRequest(
                    $"[COM-STOP ] " +
                    $"PortName:{serialPort.PortName} " +
                    $"OpenStatus:{serialPort.IsOpen} ");
                serialPort.Dispose();
                serialPort = null;
            }

            return result;
        }

        public void Send(IEnumerable<byte> datas)
        {
            datas.ToList().ForEach((data) => sendQueue.Enqueue(data));
        }
    }
}
