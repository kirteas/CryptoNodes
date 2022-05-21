using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoNodes
{
    public partial class MainWindow : Window
    {
        // Поля:
        private string url;
        private string keyPoint;
        private Dispatcher dispatcher;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            url = TEXTBOX_URL.Text;
            keyPoint = "nav-item float-left";
            dispatcher = new Dispatcher(url, keyPoint);
            BTN_All.IsEnabled = false;
            BTN_Clear.IsEnabled = false;
            BTN_Setup.IsEnabled = false;
        }

        // КНОПКА - <Сканировать>
        async private void BTN_Scan_Click(object sender, RoutedEventArgs e)
        {
            BTN_Scan.IsEnabled = false;
            try
            {
                bool Result = await dispatcher.ScanAsync();
                LISTBOX_Coins.ItemsSource = dispatcher.GetCoins();
                Console.WriteLine(dispatcher.GetCountItems());
                GROUPBOX_Coins.Header = $"Найденные валюты = {dispatcher.GetCountItems()} шт.";
                if (LISTBOX_Coins.Items.Count > 0)
                {
                    BTN_All.IsEnabled = true;
                    BTN_Clear.IsEnabled = true;
                    BTN_Setup.IsEnabled = true;
                }
                if (Result)
                {
                    BTN_Scan.IsEnabled = true;
                }
                
            }
            catch (Exception)
            {

            }
        }

        // Событие при выборе объекта (Монеты) в списке ListBox;
        private void LISTBOX_Coins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                for (int i = 0; i < dispatcher.GetCountItems(); i++)
                {
                    if (LISTBOX_Coins.SelectedItem.ToString() == dispatcher.GetCollector()[i, 0])
                    {
                        TEXTBOX_Nodes.Text = dispatcher.GetCollector()[i, 2];
                        return;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        // КНОПКА - <Всё>
        private void BTN_All_Click(object sender, RoutedEventArgs e)
        {
            if (LISTBOX_Coins.Items.Count > 0)
            {
                //foreach (var item in LISTBOX_Coins.Items)
                //{
                //    (item as CheckBox).IsChecked = true;
                //}
                LISTBOX_Coins.ItemsSource.GetEnumerator().Reset();
                for (int i = 0; i < LISTBOX_Coins.Items.Count; i++)
                {
                    //LISTBOX_Coins.ItemsSource.Cast<CheckBox>().ToArray<CheckBox>()[i].IsChecked = true;
                    
                    (LISTBOX_Coins.ItemsSource.GetEnumerator().Current as CheckBox).IsChecked = true;
                    LISTBOX_Coins.ItemsSource.GetEnumerator().MoveNext();
                }

            }
        }

        // КНОПКА - <Очистить>
        private void BTN_Clear_Click(object sender, RoutedEventArgs e)
        {
            if (LISTBOX_Coins.Items.Count > 0)
            {
                foreach (var item in LISTBOX_Coins.Items)
                {
                    (item as CheckBox).IsChecked = false;
                }
            }
        }
    }
}
