using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using MruF5100jpDummy.Model.Common;
using MruF5100jpDummy.Model.Logging;
using MruF5100jpDummy.Model.SerialInterfaceProtocol;
using MruF5100jpDummy.Model.SerialPortManager;
using System;
using System.Collections.ObjectModel;

namespace MruF5100jpDummy.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "MruF5100jpDummy";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        IEventAggregator _ea;
        LogWriter logWriter;

        public YoukyuuOutouKekka YoukyuuOutouKekka {
            get => serialInterfaceProtocolManager.YoukyuuOutouKekka;
            set { serialInterfaceProtocolManager.YoukyuuOutouKekka = value; }
        }

        public NinshouJoutai NinshouJoutai
        {
            get => serialInterfaceProtocolManager.NinshouJoutai;
            set { serialInterfaceProtocolManager.NinshouJoutai = value; }
        }

        public NinshouKanryouJoutai NinshouKanryouJoutai
        {
            get => serialInterfaceProtocolManager.NinshouKanryouJoutai;
            set { serialInterfaceProtocolManager.NinshouKanryouJoutai = value; }
        }

        public NinshouKekkaNgShousai NinshouKekkaNgShousai
        {
            get => serialInterfaceProtocolManager.NinshouKekkaNgShousai;
            set { serialInterfaceProtocolManager.NinshouKekkaNgShousai = value; }
        }

        public string NinshouJoutaiYoukyuuOutouRiyoushaId
        {
            get => serialInterfaceProtocolManager.RiyoushaId;
            set { serialInterfaceProtocolManager.RiyoushaId = value; }
        }

        public MainWindowViewModel(IEventAggregator ea)
        {
            // コマンドの準備
            SerialStartButton = new DelegateCommand(SerialStartButtonExecute);
            SerialStopButton = new DelegateCommand(SerialStopButtonExecute);
            NinshouYoukyuuCommandTestSendButton = new DelegateCommand(NinshouYoukyuuCommandTestSendButtonExecute);
            NinshouJoutaiYoukyuuCommandTestSendButton = new DelegateCommand(NinshouJoutaiYoukyuuCommandTestSendButtonExecute);
            LogClearButton = new DelegateCommand(LogClearButtonExecute);
            PortListSelectionChanged = new DelegateCommand<object[]>(PortListChangedExecute);
            IncrementYoukyuuOutouJikanMsCommand = new DelegateCommand(IncrementYoukyuuOutouJikanMsValueExecute);
            DecrementYoukyuuOutouJikanMsCommand = new DelegateCommand(DecrementYoukyuuOutouJikanMsValueExecute);
            IncrementYoukyuuJoutaiOutouJikanMsCommand = new DelegateCommand(IncrementYoukyuuJoutaiOutouJikanMsValueExecute);
            DecrementYoukyuuJoutaiOutouJikanMsCommand = new DelegateCommand(DecrementYoukyuuJoutaiOutouJikanMsValueExecute);

            // コンボボックスの準備
            SerialPortManager.GetAvailablePortNames().ForEach(serialPort =>
            {
                if (!string.IsNullOrEmpty(serialPort))
                {
                    _serialPortList.Add(new ComboBoxViewModel(Common.ExtractNumber(serialPort),serialPort));
                }
            });

            _ea = ea;
            logWriter = new LogWriter(_ea,(Log log) =>
            {
                _logItems.Add(
                    new LogItem()
                    {
                        Timestamp = log.dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Content = log.content
                    });
            });
        }

        /// ボタン関係
        /// シリアル通信関係
        public DelegateCommand SerialStartButton { get; }

        private void SerialStartButtonExecute()
        {
            if (!string.IsNullOrEmpty(selectedSerialComPort))
            {
                serialInterfaceProtocolManager.ComStart(selectedSerialComPort, new LogWriteRequester(_ea));
            }   
        }

        public DelegateCommand SerialStopButton { get; }

        private void SerialStopButtonExecute()
        {
            serialInterfaceProtocolManager.ComStop();
        }

        public DelegateCommand NinshouYoukyuuCommandTestSendButton { get; }

        // 認証要求関係

        private string _ninshouYoukyuuRiyoushaId = "00043130";

        public string NinshouYoukyuuRiyoushaId
        {
            get { return _ninshouYoukyuuRiyoushaId; }
            set { SetProperty(ref _ninshouYoukyuuRiyoushaId, value); }
        }

        private void NinshouYoukyuuCommandTestSendButtonExecute()
        {
            serialInterfaceProtocolManager.Send(new NinshouYoukyuuCommand(1,NyuutaishitsuHoukou.Nyuushitsu, _ninshouYoukyuuRiyoushaId));
        }

        public DelegateCommand NinshouJoutaiYoukyuuCommandTestSendButton { get; }

        private void NinshouJoutaiYoukyuuCommandTestSendButtonExecute()
        {
            serialInterfaceProtocolManager.Send(new NinshouJoutaiYoukyuuCommand(1, NyuutaishitsuHoukou.Nyuushitsu));
        }

        public DelegateCommand LogClearButton { get; }

        private void LogClearButtonExecute()
        {
            LogItems = new ObservableCollection<LogItem>();
        }

        /// Combo Box
        private ObservableCollection<ComboBoxViewModel> _serialPortList =
            new ObservableCollection<ComboBoxViewModel>();

        public ObservableCollection<ComboBoxViewModel> SerialPortList
        {
            get { return _serialPortList; }
            set { SetProperty(ref _serialPortList, value); }
        }

        public DelegateCommand<object[]> PortListSelectionChanged { get; }

        private void PortListChangedExecute(object[] selectedItems)
        {
            try
            {
                var selectedItem = selectedItems[0] as ComboBoxViewModel;
                selectedSerialComPort = selectedItem.DisplayValue;
            }
            catch { }
        }

        SerialInterfaceProtocolManager serialInterfaceProtocolManager = new SerialInterfaceProtocolManager();
        String selectedSerialComPort = "";

        ObservableCollection<LogItem> _logItems =
            new ObservableCollection<LogItem>();
        public ObservableCollection<LogItem> LogItems
        {
            get { return _logItems; }
            set { SetProperty(ref _logItems, value); }
        }

        // 要求応答時間
        public uint YoukyuuOutouJikanMs
        {
            get { return serialInterfaceProtocolManager.YoukyuuOutouJikanMs; }
            set { SetProperty(ref serialInterfaceProtocolManager.YoukyuuOutouJikanMs, value); }
        }

        public DelegateCommand IncrementYoukyuuOutouJikanMsCommand { get; private set; }
        public DelegateCommand DecrementYoukyuuOutouJikanMsCommand { get; private set; }

        private void IncrementYoukyuuOutouJikanMsValueExecute()
        {
            if (YoukyuuOutouJikanMs <= 9000)
            {
                YoukyuuOutouJikanMs = YoukyuuOutouJikanMs + 1000;
            }
        }

        private void DecrementYoukyuuOutouJikanMsValueExecute()
        {
            if (YoukyuuOutouJikanMs >= 1000)
            {
                YoukyuuOutouJikanMs = YoukyuuOutouJikanMs - 1000;
            }
        }

        // 要求状態応答時間
        public uint YoukyuuJoutaiOutouJikanMs
        {
            get { return serialInterfaceProtocolManager.YoukyuuJoutaiOutouJikanMs; }
            set { SetProperty(ref serialInterfaceProtocolManager.YoukyuuJoutaiOutouJikanMs, value); }
        }

        public DelegateCommand IncrementYoukyuuJoutaiOutouJikanMsCommand { get; private set; }
        public DelegateCommand DecrementYoukyuuJoutaiOutouJikanMsCommand { get; private set; }

        private void IncrementYoukyuuJoutaiOutouJikanMsValueExecute()
        {
            if(YoukyuuJoutaiOutouJikanMs <= 9000)
            {
                YoukyuuJoutaiOutouJikanMs = YoukyuuJoutaiOutouJikanMs + 1000;
            }
        }

        private void DecrementYoukyuuJoutaiOutouJikanMsValueExecute()
        {
            if(YoukyuuJoutaiOutouJikanMs >= 1000)
            {
                YoukyuuJoutaiOutouJikanMs = YoukyuuJoutaiOutouJikanMs - 1000;
            }
        }
    }
}
