using System.Text.Json;

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
        static string _filePath = "products.json";

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            LoadProducts();
            Register();
        }

        static void Exit()
        {
            SaveProducts();
            Environment.Exit(0);
        }

        static void Register()
        {
            do
            {
                Console.WriteLine("Ввійти як покупець чи адміністратор? (покупець, адміністратор)");
                Console.WriteLine();
                string choice = Console.ReadLine()!.ToLower();
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
                Console.WriteLine("2. Додати товар");
                Console.WriteLine("3. Пошук товару");
                Console.WriteLine("4. Видалити товар");
                Console.WriteLine("5. Сортування товарів");
                Console.WriteLine("6. Статистика");
                Console.WriteLine("0. Вихід");
                Console.Write("Ваш вибір: ");
                if (!int.TryParse(Console.ReadLine(), out choice)) continue;
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
                if (!int.TryParse(Console.ReadLine(), out choice)) continue;
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

        static void LoadProducts()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var loaded = JsonSerializer.Deserialize<List<Product?>>(File.ReadAllText(_filePath));
                    
                    _products.Clear();
                    if (loaded != null)
                    {
                        foreach (var p in loaded)
                        {
                            if (p != null)
                            {
                                _products.Add(p);
                            }
                        }
                    }
                } 
                catch (IOException) 
                {}
                catch (JsonException){}
            }

            if (_products.Count == 0)
            {
                string[] names = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
                double[] prices = { 500, 300, 700, 150, 250 };
                for (int i = 0; i < 5; i++)
                    _products.Add(new Product(names[i], prices[i]));   
                SaveProducts();
            }
        }

        static void SaveProducts()
        {
            File.WriteAllText(_filePath,
                JsonSerializer.Serialize(_products, new JsonSerializerOptions { WriteIndented = true }));
        }

        static void ShowProducts()
        {
            if (_products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній.");
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
                Console.WriteLine("Некоректна кількість.");
                return;
            }
            
            for (int i = 0; i < c; i++)
            {
                Console.Write($"Товар #{i+1} Назва: ");
                string n = Console.ReadLine()!;
                Console.Write($"Товар #{i+1} Ціна: ");
                
                if (!double.TryParse(Console.ReadLine(), out double p) || p < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Некоректна ціна. Спробуйте цей товар ще раз.");
                    Console.ResetColor();
                    i--;
                    continue;
                }

                _products.Add(new Product(n, p));
            }

            SaveProducts();
        }

        static void SearchProduct()
        {
            Console.Write("Введіть назву товару для пошуку: ");
            string key = Console.ReadLine()!.ToLower();
            bool found = false;
            
            Console.WriteLine("\n--- Результати пошуку ---");
            for (int i = 0; i < _products.Count; i++)
            {
                if (_products[i].Name.ToLower() == key) 
                {
                    Console.WriteLine($"Знайдено: {_products[i].Name} — {_products[i].Price} грн (Індекс {i})");
                    found = true;
                }
            }
            
            if (!found)
            {
                Console.WriteLine("Не знайдено.");
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
                    Console.WriteLine("Помилка: Невірний індекс.");
                    Console.ResetColor();
                }
            }
            else if (c == 2)
            {
                Console.Write("Введіть назву: ");
                string n = Console.ReadLine()!.ToLower();
                bool removed = false;
                
                for (int i = 0; i < _products.Count; i++)
                {
                    if (_products[i].Name.ToLower() == n)
                    {
                        string name = _products[i].Name;
                        _products.RemoveAt(i);
                        Console.WriteLine($"Видалено товар: {name}!");
                        removed = true;
                        break;
                    }
                }
                if (!removed)
                {
                    Console.WriteLine("Товар з такою назвою не знайдено.");
                }
            }
            else
            {
                Console.WriteLine("Невірний вибір.");
            }

            SaveProducts();
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
            Console.WriteLine("Відсортовано за ціною (Бульбашкове сортування).");
            ShowProducts();
        }

        static void SortByNameBuiltin()
        {
            _products.Sort((a, b) => 
                String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
            );
            Console.WriteLine("Відсортовано за назвою (Вбудований List.Sort).");
            ShowProducts();
        }

        static void ShowSortingMenu()
        {
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
            
            SaveProducts();
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
            int count = 0;
            int countAbove500 = 0;
            Product? minProduct = null;
            Product? maxProduct = null;

            foreach (var p in _products)
            {
                if (p.Price > 0)
                {
                    count++;
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

                    if (p.Price > 500)
                    {
                        countAbove500++;
                    }
                }
            }
            
            if (count == 0) 
            {
                Console.WriteLine("Немає товарів із ціною > 0 для статистики.");
                return;
            }

            double average = totalPrice / count;
            
            Console.WriteLine("\n=== СТАТИСТИКА ТОВАРІВ ===");
            Console.WriteLine($"Кількість валідних товарів: {count}");
            Console.WriteLine($"Загальна сума цін: {totalPrice:F2} грн");
            Console.WriteLine($"Середня ціна: {Math.Round(average, 2):F2} грн");        
            
            if (minProduct != null && maxProduct != null)
            {
                Console.WriteLine($"Мінімальна ціна: {minProduct.Name} — {minPrice:F2} грн");
                Console.WriteLine($"Максимальна ціна: {maxProduct.Name} — {maxPrice:F2} грн");
            }
            
            Console.WriteLine($"Кількість товарів > 500 грн: {countAbove500}");
        }

        static void BuyProducts()
        {
            if (_products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній.");
                return;
            }

            double sum = 0;
            Random rand = new Random();
            Console.WriteLine("\nДоступні товари:");
            ShowProducts();
            Console.WriteLine("Виберіть товари (індекс:кількість, або 'стоп' для завершення):");
            string input;
            do
            {
                input = Console.ReadLine()!;
                if (input.ToLower() == "стоп") break;
                var parts = input.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int idx) || !int.TryParse(parts[1], out int qty))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний формат! (приклад: 0:2(ввести 2 одиниці товару з індексом 0))");
                    Console.ResetColor();
                    continue;
                }

                if (idx < 0 || idx >= _products.Count || qty <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний індекс або кількість!");
                    Console.ResetColor();
                    continue;
                }

                sum += _products[idx].Price * qty;
            } while (true);

            if (sum > 0)
            {
                double discount = rand.Next(0, 21) / 100.0;
                double total = sum * (1 - discount);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Сума: {Math.Round(sum, 2):F2} грн");
                Console.WriteLine($"Знижка: {discount * 100:F0}%");
                Console.WriteLine($"До сплати: {Math.Round(total, 2):F2} грн");
                Console.ResetColor();
            }
            else if (sum == 0)
            {
                 Console.WriteLine("Замовлення скасовано або не містить товарів.");
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