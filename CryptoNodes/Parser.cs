using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoNodes.Dispatcher;

namespace CryptoNodes
{
    class Parser : Dispatcher.IStructParsing
    {
        // Поля:
        private string keyPoint = null; // Ключ-синтаксис (элемент/блок ID, либо class), от которого начинается парсинг страницы; (nav-item float-left);
        /* Искомый элемент-синтаксис, данные которого будут записаны в массив коллекционера. Необходимо указывать значение
         * полное название класса, без ковычек, только до символа '"' */
        private string linkItem = null; //  Ключ-синтаксис (элемент) начала парсинга; Поиск ссылок;
        private string peers = null; // тег к названию индивидуальной страницы Монеты;
        private string nodekey = null; // тег к названию участка с нодами;

        // Конструктор:
        public Parser(string keyPoint, string linkItem, string peers, string nodekey)
        {
            this.keyPoint = keyPoint;
            this.linkItem = linkItem;
            this.peers = peers;
            this.nodekey = nodekey;
        }

        // Методы:
        public void WebParse(string code, ref Collector collector)
        {
            // Если строка не пустая;
            if (code != null)
            {
                // Производим подсчёт количества найденных участков и добавляем их количество (размер) в Коллекцию;
                // В данном случаи, количество найденных участков = количеству найденных монет;
                collector.GetCountItems = GetCountElements(code);
                // Если в строке найден хотя бы один участок искомого кода;
                if (collector.GetCountItems > 0)
                {
                    int it = code.IndexOf(keyPoint);  // Точка от которой идёт парсинг, отсчёт (первое вхождение);
                    // Устанавливаем размер временного массива Коллекционера валют:
                    string[,] array = new string[collector.GetCountItems, collector.GetSize]; // Временный массив;
                    int number = 0; // Номер искомого участка (монеты);

                    int depth = 0; // Переменная для подсчёта глубины-вложенности;
                    bool exit = false; // Переменная для выхода из циклов;

                    // Цикл по всему коду;
                    for (; it < code.Length; it++)
                    {
                        if (number == collector.GetCountItems)
                        {
                            break;
                        }
                        if (code[it] == keyPoint[0])
                        {
                            if (CheckKey(keyPoint, code, it))
                            {
                                // Если ключ верный, то пропускаем его, добавляя к Итератору размер ключа;
                                it += keyPoint.Length;
                                // Если мы нашли ключ, то уже зашли в нужный блок, увеличиваем углубление на +1;
                                depth = 1;
                                //----------------------------------------------------------------------------------
                                // Цикл по найденному ключу - участок кода, необходимый для парсинга (монета);
                                for (; it < code.Length; it++)
                                {
                                    /* Обнуляем переменные для новой итерации по коду */
                                    exit = false;

                                    // 1. Первое условие - нахождение искомого объекта (ссылки);
                                    if (code[it] == linkItem[0])
                                    {
                                        // Если нашли необходимый участок кода, то выполняется условие, далее идёт считывание необходимого кода-значения между ковычке "<искомый код>";
                                        if (CheckKey(linkItem, code, it))
                                        {
                                            it += linkItem.Length;
                                            // Цикл для записи ссылки;
                                            for (; it < code.Length; it++)
                                            {
                                                if (code[it] == '"')
                                                {
                                                    // Цикл для записи информации между ковычками;
                                                    for (it += 1; it < code.Length; it++)
                                                    {
                                                        if (code[it] == '"')
                                                        {
                                                            array[number, 1] += peers;
                                                            exit = true;
                                                            break;
                                                        }
                                                        array[number, 1] += code[it];
                                                    }
                                                    if (exit)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // 2. Второе условие - нахождение конца тега ">";
                                    if (code[it] == '>')
                                    {
                                        for (it += 1; it < code.Length; it++)
                                        {
                                            if (code[it] == '<')
                                            {
                                                break;
                                            }
                                            if (((code[it] != ' ') && (code[it + 1] != ' ')) || ((code[it] != ' ') && (code[it - 1] != ' ')))
                                            {
                                                if (code[it] != '\n')
                                                {
                                                    array[number, 0] += code[it];
                                                }
                                            }
                                        }
                                    }
                                    // 3. Третье условие - нахождение начала тега "<";
                                    if (code[it] == '<')
                                    {
                                        // 3.1 Поиск вхождения в блок, углубление;
                                        if (code[it + 1] != '/')
                                        {
                                            depth++;
                                        }
                                        // 3.2 Поиск выхода из блока, подъём;
                                        if (code[it + 1] == '/')
                                        {
                                            depth--;
                                        }
                                    }
                                    // Условие выхода, когда найдена одна монета (участок кода);
                                    if (depth == 0)
                                    {
                                        // Итератор - номер найденной монеты, ведём подсчёт;
                                        number++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    collector.GetItems = array;
                }
            }
        }

        /* Метод принимает HTML-код странницы;
         * Метод подсчёта количества найденных элементов (в данном случаи - "Монет"):
         * Метод универсален для любых блоков HTML-кода, количество которых необходимо найти; 
         * Для этого необходимо указать селектор, ID, либо class принадлежащий CSS странице. */
        private int GetCountElements(string code)
        {
            // Разбиваем всю строку на подстроки (разбиение HTML CSS кода страницы на блоки), далее создаём массив строк:
            // Разделитель строк являются символ ковычки (").
            string[] source = code.Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
            // Создание запроса LINQ:
            // Находим из массива строк 1 в 1 вхождения, количество и будет размер Коллекционера для валют;
            var matchQuery = from word in source
                             where word.Equals(keyPoint, StringComparison.InvariantCultureIgnoreCase)
                             select word;
            // Возвращаем количество вхождений (найденных валют):
            return matchQuery.Count();
        }

        /* Метод проверки ключа-объекта (названия селектора, ID, либо class блока CSS), искомого в коде;
         * Пробигаем и проверяем полное соответствие названию.
         * Возврат true / false в случаи совпадения или нет. */
        private bool CheckKey(string key, string code, int it)
        {
            int match = 0; // Количество совпадений;
            for (int el = 0; el < key.Length; el++, it++)
            {
                if (code[it] == key[el])
                {
                    match++;
                }
                else
                {
                    break;
                }
            }
            if (match == key.Length)
            {
                return true;
            }
            return false;
        }

        // Метод парсинга индивидуальных страниц монет;
        async public Task GetNodesAsync(int number, string code, Collector collector)
        {
            await Task.Run(() =>
            {
                // Если строка не пустая;
                if (code != null)
                {
                    /* Поиск позиции расположения ключа нодов в коде */
                    int it = code.IndexOf(nodekey);
                    if (it > 0)
                    {
                        // Перескакиваем найденный ключ;
                        it += nodekey.Length;
                        int depth = 1; // Переменная для подсчёта глубины-вложенности;
                        bool endNode = false;
                        bool finallyNode = false;

                        // Цикл прохода по коду;
                        for (; it < code.Length; it++)
                        {
                            endNode = false;
                            finallyNode = false;

                            // 2. Второе условие, если найден конец блока '>';
                            if (code[it] == '>')
                            {
                                it++;
                                for (; it < code.Length; it++)
                                {
                                    if (code[it] == '<')
                                    {
                                        if (finallyNode)
                                        {
                                            endNode = true;
                                        }
                                        break;
                                    }
                                    // ИСПРАВИТЬ УСЛОВИЕ, ТОЛЬКО ИЗ_ЗА НЕГО НЕ ДОДЕЛАНО!!!!    ТАКЖЕ ИСПРАВИТЬ ВЛОЖЕННОСТИ, ПРОВЕРИТЬ НА ПРАВИЛЬНОСТЬ!
                                    if ((code[it] != ' ') || ((code[it] == ' ') && (code[it + 1] != ' ') && (code[it - 1] != ' ')))
                                    {
                                        if (code[it] != '\n')
                                        {
                                            collector.GetItems[number, 2] += code[it];
                                            finallyNode = true;
                                        }
                                    }
                                }
                                if (endNode)
                                {
                                    //depth--;
                                    collector.GetItems[number, 2] += '\n';
                                }
                            }
                            // 1. Первое условие, если найдено начало блока '<';
                            if (code[it] == '<')
                            {
                                if (code[it + 1] != '/')
                                {
                                    depth++;
                                }
                                else if (code[it + 1] == '/')
                                {
                                    depth--;
                                }
                            }
                            if (depth == 0)
                            {
                                break;
                            }
                        } // конец цикла считывания кода;
                    }
                }
            });
            
        }
    }
}
