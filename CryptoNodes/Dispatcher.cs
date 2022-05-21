using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoNodes
{
    class Dispatcher
    {
        // Поля класса:
        private string url = null; // Основная страница сайта (https://alltheblocks.net);
        private string keyPoint = null; // Ключ-синтаксис (элемент/блок ID, либо class), от которого начинается парсинг страницы; (nav-item float-left);
        /* Искомый элемент-синтаксис, данные которого будут записаны в массив коллекционера. Необходимо указывать значение
         * полное название класса, без ковычек, только до символа '"' */
        private string linkItem = "a href="; //  Ключ-синтаксис (элемент) начала парсинга; Поиск ссылок;
        private string peers = "/peers"; // тег к названию индивидуальной страницы Монеты;
        private string nodekey = "p-2 text-monospace"; // тег к названию участка с нодами;
        // Объекты классов:
        private WebCodes wcodes;
        private Parser parser;
        private Collector collector;
        // Коллекция Монет:
        private ObservableCollection<string> Coins;
        

        // Конструктор класса:
        public Dispatcher(string url, string keyPoint)
        {
            this.url = url;
            this.keyPoint = keyPoint;
            wcodes = new WebCodes();
            parser = new Parser(this.keyPoint, linkItem, peers, nodekey);
            collector = new Collector();
            Coins = new ObservableCollection<string>();
        }

        // Интерфейсы класса:
        public interface IWebGather
        {
            string GetWebCode(string urladress);
        }
        public interface IStructParsing
        {
            void WebParse(string code, ref Collector collector);
            Task GetNodesAsync(int number, string code, Collector collector);
        }

        // Структуры класса:
        /* Структура для хранения данных;
        * Коллекционер монет: */
        public struct Collector
        {
            // Поля:
            private const int size = 4; // Размерность массива для хранения валют. 0{название} 1{адрес страницы} 2{узлы/ноды} 3{код страницы};
            private int CountItems; // Счётчик, количество найденных валют;
            private string[,] items; // Многомерный массив для хранения валют;

            // Мутаторы:
            public int GetSize
            {
                get { return size; }
            }

            public int GetCountItems
            {
                get { return CountItems; }
                set { CountItems = value; }
            }

            public string[,] GetItems
            {
                get { return items; }
                set { items = value; }
            }
        }

        // Методы класса:
        /* Метод сканирования и парсинга страниц HTML-кода сайта; */
        async public Task<bool> ScanAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    // Вначале необходимо выполнить парсинг основной  (главной) страницы, чтобы собрать список всех монет и их ссылки на индивидуальные страницы;
                    parser.WebParse(wcodes.GetWebCode(url), ref collector);
                    // Если сканирование ранее уже запускали, то необходимо пересоздать список коллекции монет;
                    if (Coins.Count > 0)
                    {
                        Coins = new ObservableCollection<string>();
                    }
                    /* Выполняем Асинхронно парсинг страницы каждой монеты;
                     * Производим поиск Нодов каждой монеты;
                     * Записываем в [4] ячейку массива Коллекционера HTML-код каждой монеты; */
                    for (int i = 0; i < collector.GetCountItems; i++)
                    {
                        collector.GetItems[i, 3] = wcodes.GetWebCode(url + collector.GetItems[i, 1]);
                        parser.GetNodesAsync(i, collector.GetItems[i, 3], collector); // Если использовать 'async' перед методом, то возможна ошибка отставания сканирования, быстрее начнётся следующий цикл;
                    }
                    // Отладочная часть для вывода информации в консоль;
                    // Добавление в список (коллекцию) объектов - монет; 
                    for (int i = 0; i < collector.GetCountItems; i++)
                    {
                        Coins.Add(collector.GetItems[i, 0]);
                        Console.WriteLine(collector.GetItems[i, 0]);
                        Console.WriteLine(collector.GetItems[i, 1]);
                        //for (int j = 0; j < collector.GetItems[i, 2].Length; j++)
                        //{
                        //    Console.Write(collector.GetItems[i, 2][j]);
                        //}
                        //Console.WriteLine();
                    }
                });
            }
            catch (Exception)
            {

            }
            return true;
        }


        /* Метод доступа к структуре <Коллекционер> */
        public string[,] GetCollector()
        {
            return collector.GetItems;
        }

        /* Метод доступа к размеру структуру <Коллекционер> */
        public int GetCountItems()
        {
            return collector.GetCountItems;
        }

        /* Метод доступа к списку коллекции Монет */
        public ObservableCollection<string> GetCoins()
        {
            return Coins;
        }
    }
}
