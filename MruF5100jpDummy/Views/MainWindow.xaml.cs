﻿using Prism.Events;
using MruF5100jpDummy.Model.Logging;
using System.Windows;

namespace MruF5100jpDummy.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IEventAggregator ea)
        {
            InitializeComponent();
            itemListBox.Loaded += MyListBox_Loaded;
            ea.GetEvent<LogUpdated>().Subscribe((value) => ScrollToBottom());
        }

        private void MyListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (itemListBox.Items.Count > 0)
            {
                var lastItem = itemListBox.Items[itemListBox.Items.Count - 1];
                itemListBox.ScrollIntoView(lastItem);
            }
        }
    }
}
