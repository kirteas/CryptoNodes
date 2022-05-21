using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CryptoNodes
{
    class WebCodes : Dispatcher.IWebGather
    {
        // Поля класса:
        private string code;
        // Объекты для работы с WEB-запросами и патоками:
        private HttpWebRequest wrequest;
        private HttpWebResponse wresponse;
        private StreamReader streamreader;

        // Конструктор класса:
        public WebCodes()
        {
            code = null;
            wrequest = null;
            wresponse = null;
            streamreader = null;
        }

        // Методы класса:
        /* Метод получает URL-адрес страницы, затем возвращает HTML-код этой страницы в строковой переменной, 
         * в случаи отказа или ошибки доступа возвращает <null> значение */
        public string GetWebCode(string urladress)
        {
            wrequest = (HttpWebRequest)HttpWebRequest.Create(urladress);
            wrequest.Method = "GET";

            wresponse = (HttpWebResponse)(wrequest.GetResponse());
            streamreader = new StreamReader(wresponse.GetResponseStream(), Encoding.Default);
            code = streamreader.ReadToEnd();

            if ((code.Length > 0) && (code != null))
            {
                return code;
            }
            return null;
        }
    }
}
