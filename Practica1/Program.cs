using System.Security.Cryptography;
using System.Text;


namespace Practica_1
{
    class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public Product(int id, string name, double price)
        {
            this.Id = id;
            this.Name = name;  
            this.Price = price;
        }

        public string ToCsvString() => $"{Id},{Name},{Price:F2}".Replace(',', '.'); 
        
        public static Product? FromCsvLine(string line)
        {
            var parts = line.Split(',');
            if (parts.Length != 3) return null;

            try
            {
                if (int.TryParse(parts[0], out int id) && 
                    double.TryParse(parts[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double price))
                {
                    return new Product(id, parts[1], price);
                }
            }
            catch(FormatException) {}
            catch(OverflowException) { }
            return null;
        }
    }

    class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }

        public User(int id, string email, string hashedPassword)
        {
            this.Id = id;
            this.Email = email;
            this.HashedPassword = hashedPassword;
        }

        public string ToCsvString() => $"{Id},{Email},{HashedPassword}";

        public static User? FromCsvLine(string line)
        {
            var parts = line.Split(',');
            if (parts.Length != 3) return null;

            try
            {
                if (int.TryParse(parts[0], out int id))
                {
                    return new User(id, parts[1], parts[2]);
                }
            }
            catch(FormatException) { }
            catch(OverflowException) { }
            return null;
        }
    }

    static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }

    static class CsvManager
    {
        public const string ProductHeader = "Id,Name,Price";
        public const string UserHeader = "Id,Email,HashedPassword";
        public const string ProductPath = "products.csv";
        public const string UserPath = "users.csv";

        public static void EnsureFileExists(string filePath, string header)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, header + Environment.NewLine);
            }
        }
        
        public static void WriteAll<T>(string filePath, string header, List<T> items) where T : class
        {
            List<string> lines = new List<string> { header };
            foreach (var item in items)
            {
                if (item is Product p) lines.Add(p.ToCsvString());
                else if (item is User u) lines.Add(u.ToCsvString());
            }
            try
            {
                File.WriteAllLines(filePath, lines);
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка запису у файл: {filePath}");
                Console.ResetColor();
            }
        }

        public static List<T> ReadAll<T>(string filePath, string expectedHeader, Func<string, T?> parser) where T : class
        {
            List<T> items = new List<T>();

            if (!File.Exists(filePath)) return items;

            try
            {
                var lines = File.ReadAllLines(filePath);

                if (lines.Length == 0 || lines[0].Trim() != expectedHeader)
                {
                    return items;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    
                    T? item = parser(lines[i]);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
            catch (IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка читання файлу: {filePath}. Буде використано порожній список.");
                Console.ResetColor();
            }

            return items;
        }
        
        public static int GenerateNewId(string filePath)
        {
            if (!File.Exists(filePath)) return 1;

            int maxId = 0;
            try
            {
                var lines = File.ReadAllLines(filePath);
                
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 1) continue;

                    if (int.TryParse(parts[0], out int id))
                    {
                        if (id > maxId)
                            maxId = id;
                    }
                }
            }
            catch (IOException)
            {
                return 1;
            }

            return maxId + 1;
        }
    }

    static class Program
    {
        static List<Product> _products = new();
        static List<User> _users = new();
        
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            CsvManager.EnsureFileExists(CsvManager.ProductPath, CsvManager.ProductHeader);
            CsvManager.EnsureFileExists(CsvManager.UserPath, CsvManager.UserHeader);
            
            LoadData();
            Register();
        }

        static void Exit()
        {
            Environment.Exit(0);
        }

        static void LoadData()
        {
            _products = CsvManager.ReadAll(CsvManager.ProductPath, CsvManager.ProductHeader, Product.FromCsvLine);
            _users = CsvManager.ReadAll(CsvManager.UserPath, CsvManager.UserHeader, User.FromCsvLine);

            if (_users.Count == 0)
            {
                string adminEmail = "admin@store.com";
                string adminPass = "admin";
                
                User admin = new User(
                    CsvManager.GenerateNewId(CsvManager.UserPath),
                    adminEmail,
                    PasswordHasher.Hash(adminPass)
                );
                _users.Add(admin);
                CsvManager.WriteAll(CsvManager.UserPath, CsvManager.UserHeader, _users);
            }
            
            if (_products.Count == 0)
            {
                string[] names = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
                double[] prices = { 500, 300, 700, 150, 250 };
                for (int i = 0; i < 5; i++)
                {
                     int newId = CsvManager.GenerateNewId(CsvManager.ProductPath);
                    _products.Add(new Product(newId, names[i], prices[i]));   
                }
                SaveProducts();
            }
        }
        
        static void SaveProducts()
        {
            CsvManager.WriteAll(CsvManager.ProductPath, CsvManager.ProductHeader, _products);
        }
        
        static void RegisterUser()
        {
            Console.Clear();
            Console.WriteLine("=== РЕЄСТРАЦІЯ ===");
            
            Console.Write("Введіть email (буде Ваш логін): ");
            string email = Console.ReadLine()!.Trim().ToLower();

            bool emailExists = false;
            foreach (var user in _users)
            {
                if (user.Email == email)
                {
                    emailExists = true;
                    break;
                }
            }

            if (emailExists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Користувач з таким email вже існує.");
                Console.ResetColor();
                Console.WriteLine("Натисніть будь-яку клавішу...");
                Console.ReadKey(true);
                return;
            }

            Console.Write("Введіть пароль: ");
            string password = Console.ReadLine()!;
            
            string hashedPassword = PasswordHasher.Hash(password);
            int newId = CsvManager.GenerateNewId(CsvManager.UserPath);
            
            User newUser = new User(newId, email, hashedPassword);
            _users.Add(newUser);
            
            CsvManager.WriteAll(CsvManager.UserPath, CsvManager.UserHeader, _users);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Реєстрація успішна! Тепер Ви можете ввійти.");
            Console.ResetColor();
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
        }
        
        static void Register()
        {
            do
            {
                Console.WriteLine("Ввійти як покупець, адміністратор, чи 'реєстрація'? (покупець, адміністратор, реєстрація)");
                Console.WriteLine(" ");
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
                else if (choice == "реєстрація")
                {
                    RegisterUser();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний вибір! Спробуйте ще раз.");
                    Console.ResetColor();
                    Console.WriteLine("Натисніть будь-яку клавішу...");
                    Console.ReadKey(true);
                }
            } while (true);
        }

        static void Login()
        {
            int attempts = 0;
            do
            {
                Console.Clear();
                Console.WriteLine("=== АВТОРИЗАЦІЯ ===");
                Console.Write("Введіть email: ");
                string enteredEmail = Console.ReadLine()!.Trim().ToLower();
                Console.Write("Введіть пароль: ");
                string enteredPassword = Console.ReadLine()!;
                
                string hashedEnteredPassword = PasswordHasher.Hash(enteredPassword);

                User? foundUser = null;
                foreach (var user in _users)
                {
                    if (user.Email == enteredEmail && user.HashedPassword == hashedEnteredPassword)
                    {
                        foundUser = user;
                        break;
                    }
                }

                if (foundUser != null)
                { 
                    Console.WriteLine("Вхід успішний!");
                    
                    if (foundUser.Email == "admin@store.com") 
                    {
                        AdminShowMenu();
                    }
                    else
                    {
                        ShowMenu();
                    }
                    return;
                }
                else
                {
                    attempts++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Невірний email або пароль! {attempts} спроба(и) з 3.");
                    Console.ResetColor();
                    Console.WriteLine("Натисніть будь-яку клавішу...");
                    Console.ReadKey(true);
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
                Console.Clear();
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
                Console.Clear();
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

        static void ShowProducts()
        {
            Console.Clear();
            if (_products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній.");
                Console.WriteLine("Натисніть будь-яку клавішу...");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine("\n------------------------------------------------");
            Console.WriteLine($"| {"ID",-5} | {"Назва",-25} | {"Ціна",10} |");
            Console.WriteLine("------------------------------------------------");
            
            foreach (var p in _products)
            {
                Console.WriteLine($"| {p.Id,-5} | {p.Name,-25} | {p.Price,10:F2} |");
            }

            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
        }

        static void AddProducts()
        {
            Console.Clear();
            Console.Write("Скільки товарів додати: ");
            if (!int.TryParse(Console.ReadLine(), out int c) || c <= 0) 
            {
                Console.WriteLine("Некоректна кількість.");
                Console.ReadKey(true);
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

                int newId = CsvManager.GenerateNewId(CsvManager.ProductPath);
                _products.Add(new Product(newId, n, p));
            }

            SaveProducts();
        }

        static void SearchProduct()
        {
            Console.Clear();
            Console.Write("Введіть назву товару для пошуку: ");
            string key = Console.ReadLine()!.ToLower();
            bool found = false;
            
            Console.WriteLine("\n--- Результати пошуку ---");
            foreach (var p in _products)
            {
                if (p.Name.ToLower() == key) 
                {
                    Console.WriteLine($"Знайдено: {p.Name} — {p.Price} грн (ID {p.Id})");
                    found = true;
                }
            }
            
            if (!found)
            {
                Console.WriteLine("Не знайдено.");
            }
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
        }

        static void DeleteProduct()
        {
            Console.Clear();
            ShowProducts();
            if (_products.Count == 0) return;
            
            Console.WriteLine("\n1. За ID");
            Console.WriteLine("2. За назвою");
            Console.Write("Вибір: ");
            if (!int.TryParse(Console.ReadLine(), out int c)) 
            {
                Console.WriteLine("Невірний вибір.");
                Console.ReadKey(true);
                return;
            }
            
            if (c == 1)
            {
                Console.Write("Введіть ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id)) return;
                
                Product? productToRemove = null;
                int indexToRemove = -1;
                
                for (int i = 0; i < _products.Count; i++)
                {
                    if (_products[i].Id == id)
                    {
                        productToRemove = _products[i];
                        indexToRemove = i;
                        break;
                    }
                }

                if (productToRemove != null)
                {
                    _products.RemoveAt(indexToRemove);
                    Console.WriteLine($"Видалено товар: {productToRemove.Name} (ID {id})!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Помилка: Товар з таким ID не знайдено.");
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
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
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
            Console.Clear();
            Console.WriteLine("\n=== МЕНЮ СОРТУВАННЯ ===");
            Console.WriteLine("1. За ціною (Власний алгоритм - Бульбашкове)");
            Console.WriteLine("2. За назвою (Вбудований List.Sort)");
            Console.Write("Вибір: ");
            
            if (!int.TryParse(Console.ReadLine(), out int c)) 
            {
                Console.WriteLine("Невірний вибір.");
                Console.ReadKey(true);
                return;
            }
            
            if (c == 1) SortByPriceBubble();
            else if (c == 2) SortByNameBuiltin();
            else Console.WriteLine("Невірний вибір.");
            
            SaveProducts();
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
        }
        
        static void ShowStatistics()
        {
            Console.Clear();
            if (_products.Count == 0) 
            {
                Console.WriteLine("Список товарів порожній. Статистика недоступна.");
                Console.ReadKey(true);
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
                Console.ReadKey(true);
                return;
            }

            double average = totalPrice / count;
            
            Console.WriteLine("\n=== СТАТИСТИКА ТОВАРІВ ===");
            Console.WriteLine($"Кількість валідних товарів: {count}");
            Console.WriteLine($"Загальна сума цін: {totalPrice:F2} грн");
            Console.WriteLine($"Середня ціна: {Math.Round(average, 2):F2} грн");        
            
            if (minProduct != null && maxProduct != null)
            {
                Console.WriteLine($"Мінімальна ціна: {minProduct.Name} (ID {minProduct.Id}) — {minPrice:F2} грн");
                Console.WriteLine($"Максимальна ціна: {maxProduct.Name} (ID {maxProduct.Id}) — {maxPrice:F2} грн");
            }
            
            Console.WriteLine($"Кількість товарів > 500 грн: {countAbove500}");
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
        }

        static void BuyProducts()
        {
            Console.Clear();
            if (_products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній.");
                Console.ReadKey(true);
                return;
            }

            double sum = 0;
            Random rand = new Random();
            Console.WriteLine("\nДоступні товари:");
            ShowProducts();
            Console.WriteLine("Виберіть товари (ID:кількість, або 'стоп' для завершення):");
            string input;
            do
            {
                input = Console.ReadLine()!;
                if (input.ToLower() == "стоп") break;
                
                var parts = input.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int id) || !int.TryParse(parts[1], out int qty))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний формат! (приклад: 1:2)");
                    Console.ResetColor();
                    continue;
                }

                Product? selectedProduct = null;
                foreach (var p in _products)
                {
                    if (p.Id == id)
                    {
                        selectedProduct = p;
                        break;
                    }
                }

                if (selectedProduct == null || qty <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний ID або кількість!");
                    Console.ResetColor();
                    continue;
                }

                sum += selectedProduct.Price * qty;
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
            Console.WriteLine("Натисніть будь-яку клавішу...");
            Console.ReadKey(true);
        }

        static void GenerateReport()
        {
            Console.Clear();
            Console.WriteLine("\n=== ЗВІТ МАГАЗИНУ ===");
            ShowProducts();
            ShowStatistics();
        }
    }
}