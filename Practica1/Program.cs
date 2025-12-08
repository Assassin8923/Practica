using System.Text;

namespace Practica_1
{
    static class Program
    {
        class Product
        {
            public string Name { get; set; }
            public double Price { get; set; }

            public Product(string name, double price)
            {
                this.Name = name;
                this.Price = price;
            }
        }

        static List<Product> _products = new();
        
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            InitializeProducts();
            Register();
        }

        static void InitializeProducts()
        {
            string[] names = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
            double[] prices = { 500, 300, 700, 150, 250 };
            
            for (int i = 0; i < names.Length; i++)
            {
                _products.Add(new Product(names[i], prices[i]));   
            }
            
        }

        static void Exit()
        {
            Environment.Exit(0);
        }

        static void Register()
        {
            do
            {
                Console.WriteLine("Ввійти як покупець чи адміністратор? (покупець, адміністратор)");
                Console.WriteLine();
                string? choice = Console.ReadLine()?.ToLower();
                if (choice == "покупець")
                {
                    ShowMenu();
                    break;
                }
                else if (choice == "адміністратор")
                {
                    Login();
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний вибір! Спробуйте ще раз.");
                    Console.ResetColor();
                }
            } while (true);
        }

        static void Login()
        {
            int attempts = 0;
            do
            {
                Console.Write("Введіть логін: ");
                string enteredLogin = Console.ReadLine()!;
                Console.Write("Введіть пароль: ");
                string enteredPassword = Console.ReadLine()!;
                if (enteredLogin == "admin" && enteredPassword == "admin")
                {
                    Console.WriteLine("Вхід успішний!");
                    AdminShowMenu();
                    return;
                }
                else
                {
                    attempts++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Невірний логін або пароль! {attempts} спроба(и) з 3.");
                    Console.ResetColor();
                }
            } while (attempts < 3);

            Console.WriteLine("Забагато невдалих спроб. Спробуйте пізніше.");
            Exit();
        }

        static void AdminShowMenu()
        {
            int choice;
            while (true)
            {
                Console.WriteLine("\n=== МЕНЮ АДМІНІСТРАТОРА ===");
                Console.WriteLine("1. Переглянути товари");
                Console.WriteLine("2. Додати товари");
                Console.WriteLine("3. Пошук товару");
                Console.WriteLine("4. Видалити товар");
                Console.WriteLine("5. Сортування товарів");
                Console.WriteLine("6. Статистика");
                Console.WriteLine("0. Вихід");
                Console.Write("Ваш вибір: ");
                
                if (!int.TryParse(Console.ReadLine(), out choice)) 
                {
                    Console.WriteLine("Невірний вибір. Введіть число.");
                    continue;
                }
                
                switch (choice)
                {
                    case 1: ShowProducts(); break;
                    case 2: AddProducts(); break;
                    case 3: SearchProduct(); break;
                    case 4: DeleteProduct(); break;
                    case 5: ShowSortingMenu(); break;
                    case 6: ShowStatistics(); break;
                    case 0: return;
                    default: Console.WriteLine("Невірний вибір!"); break;
                }
            }
        }

        static void ShowMenu()
        {
            int choice;
            while (true)
            {
                Console.WriteLine("\n=== МЕНЮ ПОКУПЦЯ ===");
                Console.WriteLine("1. Переглянути товари");
                Console.WriteLine("2. Купити товари");
                Console.WriteLine("3. Статистика");
                Console.WriteLine("4. Звіт");
                Console.WriteLine("0. Вихід");
                Console.Write("Ваш вибір: ");
                
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Невірний вибір. Введіть число.");
                    continue;
                }

                switch (choice)
                {
                    case 1: ShowProducts(); break;
                    case 2: BuyProducts(); break;
                    case 3: ShowStatistics(); break;
                    case 4: GenerateReport(); break;
                    case 0: return;
                    default: Console.WriteLine("Невірний вибір!"); break;
                }
            }
        }

        static void ShowProducts()
        {
            if (_products.Count == 0)
            {
                Console.WriteLine("\nСписок товарів порожній.");
                return;
            }

            Console.WriteLine("\n-------------------------------------------");
            Console.WriteLine($"| {"Індекс",-5} | {"Назва",-25} | {"Ціна",10} |");
            Console.WriteLine("-------------------------------------------");
            for (int i = 0; i < _products.Count; i++)
            {
                var p = _products[i];
                Console.WriteLine($"| {i,-5} | {p.Name,-25} | {p.Price,10:F2} |");
            }

            Console.WriteLine("-------------------------------------------");
        }

        static void AddProducts()
        {
            Console.Write("Скільки товарів додати: ");
            
            if (!int.TryParse(Console.ReadLine(), out int c) || c <= 0) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Некоректна кількість.");
                Console.ResetColor();
                return;
            }
            
            for (int i = 0; i < c; i++)
            {
                Console.Write($"Товар #{i+1} Назва: ");
                string n = Console.ReadLine()!.Trim(); 
                
                if (string.IsNullOrWhiteSpace(n))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Назва товару не може бути порожньою. Спробуйте цей товар ще раз.");
                    Console.ResetColor();
                    i--;
                    continue;
                }
                
                Console.Write($"Товар #{i+1} Ціна: ");
                
                if (!double.TryParse(Console.ReadLine(), out double p) || p < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Некоректна ціна (повинна бути невід'ємним числом). Спробуйте цей товар ще раз.");
                    Console.ResetColor();
                    i--;
                    continue;
                }

                _products.Add(new Product(n, p));
            }

            Console.WriteLine($"Успішно додано {c} товар(ів).");
        }

        static void SearchProduct()
        {
            Console.Write("Введіть назву товару для пошуку: ");
            string key = Console.ReadLine()!.ToLower().Trim();
            bool found = false;
            
            Console.WriteLine("\n--- Результати пошуку ---");
            for (int i = 0; i < _products.Count; i++)
            {
                if (_products[i].Name.ToLower().Contains(key)) 
                {
                    Console.WriteLine($"Знайдено: {_products[i].Name} — {_products[i].Price:F2} грн (Індекс {i})");
                    found = true;
                }
            }
            
            if (!found)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Не знайдено.");
                Console.ResetColor();
            }
        }

        static void DeleteProduct()
        {
            ShowProducts();
            if (_products.Count == 0) return;
            
            Console.WriteLine("\n1. За індексом");
            Console.WriteLine("2. За назвою");
            Console.Write("Вибір: ");
            
            if (!int.TryParse(Console.ReadLine(), out int c)) 
            {
                Console.WriteLine("Невірний вибір.");
                return;
            }
            
            if (c == 1)
            {
                Console.Write("Введіть індекс: ");
                if (!int.TryParse(Console.ReadLine(), out int id)) return;
                
                if (id >= 0 && id < _products.Count)
                {
                    string name = _products[id].Name;
                    _products.RemoveAt(id);
                    Console.WriteLine($"Видалено товар: {name}!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Помилка: Невірний індекс. Елемент не існує.");
                    Console.ResetColor();
                }
            }
            else if (c == 2)
            {
                Console.Write("Введіть назву (будуть видалені всі збіги): ");
                string n = Console.ReadLine()!.ToLower().Trim();
                int count = 0;
                
                for (int i = _products.Count - 1; i >= 0; i--)
                {
                    if (_products[i].Name.ToLower() == n)
                    {
                        string name = _products[i].Name;
                        _products.RemoveAt(i);
                        Console.WriteLine($"Видалено товар: {name}!");
                        count++;
                    }
                }
                if (count == 0)
                {
                    Console.WriteLine("Товар з такою назвою не знайдено.");
                }
            }
            else
            {
                Console.WriteLine("Невірний вибір.");
            }
        }

        static void SortByPriceBubble()
        {
            int n = _products.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (_products[j].Price > _products[j + 1].Price)
                    {
                        Product temp = _products[j];
                        _products[j] = _products[j + 1];
                        _products[j + 1] = temp;
                    }
                }
            }
            Console.WriteLine("Відсортовано за ціною (Власний алгоритм - Бульбашкове сортування).");
            ShowProducts();
        }

        static void SortByNameBuiltin()
        {
            _products.Sort((a, b) => 
                string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
            );
            Console.WriteLine("Відсортовано за назвою (Вбудований List.Sort).");
            ShowProducts();
        }

        static void ShowSortingMenu()
        {
            if (_products.Count < 2)
            {
                Console.WriteLine("Недостатньо елементів для сортування (потрібно мінімум 2).");
                return;
            }

            Console.WriteLine("\n=== МЕНЮ СОРТУВАННЯ ===");
            Console.WriteLine("1. За ціною (Власний алгоритм - Бульбашкове)");
            Console.WriteLine("2. За назвою (Вбудований List.Sort)");
            Console.Write("Вибір: ");
            
            if (!int.TryParse(Console.ReadLine(), out int c)) 
            {
                Console.WriteLine("Невірний вибір.");
                return;
            }
            
            if (c == 1) SortByPriceBubble();
            else if (c == 2) SortByNameBuiltin();
            else Console.WriteLine("Невірний вибір.");
        }
        
        static void ShowStatistics()
        {
            if (_products.Count == 0) 
            {
                Console.WriteLine("Список товарів порожній. Статистика недоступна.");
                return;
            }
            
            double totalPrice = 0;
            double minPrice = double.MaxValue;
            double maxPrice = double.MinValue;
            Product? minProduct = null;
            Product? maxProduct = null;

            foreach (var p in _products)
            {
                totalPrice += p.Price;

                if (p.Price < minPrice)
                {
                    minPrice = p.Price;
                    minProduct = p;
                }
                
                if (p.Price > maxPrice)
                {
                    maxPrice = p.Price;
                    maxProduct = p;
                }
            }
            
            double average = _products.Count > 0 ? totalPrice / _products.Count : 0;
            
            Console.WriteLine("\n=== СТАТИСТИКА ТОВАРІВ ===");
            Console.WriteLine($"Кількість товарів: {_products.Count}");
            Console.WriteLine($"Загальна сума цін: {totalPrice:F2} грн");
            Console.WriteLine($"Середня ціна: {Math.Round(average, 2):F2} грн");        
            
            if (minProduct != null && maxProduct != null)
            {
                Console.WriteLine($"Мінімальна ціна: {minProduct.Name} — {minPrice:F2} грн");
                Console.WriteLine($"Максимальна ціна: {maxProduct.Name} — {maxPrice:F2} грн");
            }
        }

        static void BuyProducts()
        {
            if (_products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній. Немає чого купувати.");
                return;
            }

            double sum = 0;
            Random rand = new Random();
            Console.WriteLine("\nДоступні товари:");
            ShowProducts();
            Console.WriteLine("Виберіть товари (індекс:кількість, або 'стоп' для завершення):");
            string? input;
            
            var purchasedItems = new Dictionary<string, (int Qty, double Price)>(); 

            do
            {
                input = Console.ReadLine();
                if (input == null || input.ToLower() == "стоп") break;
                
                var parts = input.Split(':');
                
                if (parts.Length != 2 || !int.TryParse(parts[0].Trim(), out int idx) || !int.TryParse(parts[1].Trim(), out int qty))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний формат! (приклад: 0:2)");
                    Console.ResetColor();
                    continue;
                }

                if (idx < 0 || idx >= _products.Count || qty <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний індекс, або кількість повинна бути більшою за нуль!");
                    Console.ResetColor();
                    continue;
                }
                
                var product = _products[idx];
                double itemTotal = product.Price * qty;
                sum += itemTotal;
                
                if (purchasedItems.ContainsKey(product.Name))
                {
                    purchasedItems[product.Name] = (purchasedItems[product.Name].Qty + qty, product.Price);
                }
                else
                {
                    purchasedItems.Add(product.Name, (qty, product.Price));
                }

            } while (true);

            if (sum > 0)
            {
                double discount = rand.Next(0, 21) / 100.0;
                double total = sum * (1 - discount);
                
                Console.WriteLine("\n=== ВАШ ЧЕК ===");
                Console.WriteLine("-------------------------------------------");
                foreach (var item in purchasedItems)
                {
                    Console.WriteLine($"{item.Key,-25} x{item.Value.Qty,-3} @ {item.Value.Price:F2} = {(item.Value.Qty * item.Value.Price):F2} грн");
                }
                Console.WriteLine("-------------------------------------------");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Сума без знижки: {Math.Round(sum, 2):F2} грн");
                Console.WriteLine($"Знижка ({discount * 100:F0}%): -{Math.Round(sum * discount, 2):F2} грн");
                Console.WriteLine($"ДО СПЛАТИ: {Math.Round(total, 2):F2} грн");
                Console.ResetColor();
                Console.WriteLine("Дякуємо за покупку!");
            }
            else
            {
                 Console.WriteLine("Замовлення скасовано.");
            }
        }

        static void GenerateReport()
        {
            Console.WriteLine("\n=== ЗВІТ МАГАЗИНУ ===");
            ShowProducts();
            ShowStatistics();
        }
    }
}