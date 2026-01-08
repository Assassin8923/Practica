using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.Reflection;

namespace Practica_1
{
    static class Program
    {
        public class CsvDataService<T> where T : class
        {
            private readonly string _filepath;
            private readonly string _header;
            private readonly Func<string, T> _parser;
            private readonly Func<T, string> _serializer;
            
            private readonly Action<T, int>? _idSetter; 

            public CsvDataService(
                string filepath, 
                string header, 
                Func<string, T> parser, 
                Func<T, string> serializer,
                Action<T, int>? idSetter = null)
            {
                _filepath = filepath;
                _header = header;
                _parser = parser;
                _serializer = serializer;
                _idSetter = idSetter;

                InitializeFile();
            }

            private void InitializeFile()
            {
                if (!File.Exists(_filepath))
                {
                    try
                    {
                        File.WriteAllText(_filepath, _header + Environment.NewLine);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Файл {_filepath} успішно створено.");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Помилка створення файлу: {ex.Message}");
                        Console.ResetColor();
                        throw;
                    }
                }
                else
                {
                    var firstLine = File.ReadLines(_filepath).FirstOrDefault();
                    if (firstLine?.Trim() != _header.Trim())
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Увага: Некоректна шапка у файлі {_filepath}. Очікується '{_header}'.");
                        Console.ResetColor();
                    }
                }
            }

            public List<T> AllRead()
            {
                var records = new List<T>();
                try
                {
                    var lines = File.ReadLines(_filepath).Skip(1);
                    int lineNumber = 1;

                    foreach (var line in lines)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var record = _parser(line);
                            records.Add(record);
                        }
                        catch (FormatException)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Помилка формату в рядку {lineNumber} файлу {_filepath}. Рядок пропущено: {line}.");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Непередбачувана помилка в рядку {lineNumber} файлу {_filepath}: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Критична помилка при читанні файлу {_filepath}: {ex.Message}");
                    Console.ResetColor();
                }
                return records.ToList(); 
            }
            
            public void Add(T record)
            {
                var records = AllRead();
                records.Add(record);

                ReindexAndWriteAll(records);
            }

            private void ReindexAndWriteAll(List<T> records)
            {
                if (_idSetter == null)
                {
                    WriteAll(records); 
                    return;
                }
                
                for (int i = 0; i < records.Count; i++)
                {
                    _idSetter(records[i], i + 1);
                }

                WriteAll(records);
            }
            
            public void WriteAll(List<T> records)
            {
                try
                {
                    var linesToWrite = new List<string> { _header };
                    linesToWrite.AddRange(records.Select(_serializer)); 
                    File.WriteAllLines(_filepath, linesToWrite);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Критична помилка при перезаписі файлу: {ex.Message}");
                    Console.ResetColor();
                    throw;
                }
            }
        }

        public static class PasswordHasher
        {
            public static string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var builder = new StringBuilder();
                    foreach (var b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }

            public static bool VerifyPassword(string password, string hashedPassword)
            {
                var newHash = HashPassword(password);
                return newHash == hashedPassword;
            }
        }

        public class User
        {
            public int Id { get; set; }
            public string? Email { get; set; }
            public string? HashedPassword { get; set; }
            public string? Role { get; set; }
            
            public static void SetId(User user, int newId)
            {
                user.Id = newId;
            }

            public string ToCsvString()
            {
                return $"{Id},{Email ?? string.Empty},{HashedPassword ?? string.Empty},{Role ?? string.Empty}";
            }

            public static User FromCsvString(string csvLine)
            {
                var parts = csvLine.Split(',');
                if (parts.Length != 4)
                    throw new FormatException("Неправильна кількість полів для User.");

                return new User
                {
                    Id = int.Parse(parts[0]),
                    Email = parts[1],
                    HashedPassword = parts[2],
                    Role = parts[3]
                };
            }
        }

        public class Product
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
            
            public static void SetId(Product product, int newId)
            {
                product.Id = newId;
            }

            public string ToCsvString()
            {
                return $"{Id},{Name},{Price.ToString(CultureInfo.InvariantCulture)}";
            }

            public static Product FromCsvString(string csvLine)
            {
                var parts = csvLine.Split(',');
                if (parts.Length != 3)
                    throw new FormatException("Неправильна кількість полів для Product.");

                return new Product(
                    id: int.Parse(parts[0]),
                    name: parts[1],
                    price: double.Parse(parts[2], CultureInfo.InvariantCulture)
                );
            }
        }

        static CsvDataService<Product>? _productService;
        static CsvDataService<User>? _userService;

        const string ProductHeader = "ID,Name,Price";
        const string UserHeader = "ID,Email,HashedPassword,Role";

        static User? _currentUser; 
        static bool _isGuestMode;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            try
            {
                _productService = new CsvDataService<Product>(
                    "products.csv",
                    ProductHeader,
                    Product.FromCsvString,
                    prod => prod.ToCsvString(),
                    idSetter: Product.SetId 
                );

                _userService = new CsvDataService<User>(
                    "users.csv",
                    UserHeader,
                    User.FromCsvString,
                    u => u.ToCsvString(),
                    idSetter: User.SetId
                );
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nНе вдалося ініціалізувати файлові сервіси. Програма завершує роботу.");
                Console.ResetColor();
                Exit();
                return;
            }

            InitializeProducts();
            Register(); 
        }

        static void InitializeProducts()
        {
            var products = _productService!.AllRead();
            if (products.Count > 0) return;

            Console.WriteLine("Додавання початкових товарів...");

            string[] names = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
            double[] prices = { 500, 300, 700, 150, 250 };

            for (int i = 0; i < names.Length; i++)
            {
                var product = new Product(id: 0, names[i], prices[i]); 
                _productService.Add(product); 
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
                _isGuestMode = false;
                _currentUser = null;
                Console.WriteLine("\n=== АВТЕНТИФІКАЦІЯ ===");
                Console.WriteLine("1. Увійти");
                Console.WriteLine("2. Зареєструватися");
                Console.WriteLine("3. Увійти як гість");
                Console.WriteLine("0. Вихід");
                Console.Write("Ваш вибір: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний вибір. Введіть число.");
                    Console.ResetColor();
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        if (LoginUserAndRoute()) return;
                        break;
                    case 2:
                        if (RegisterUser("User"))
                        {
                            LoginUserAfterRegistration();
                            if (_currentUser != null) 
                            {
                                if (_currentUser.Role == "Admin") AdminShowMenu();
                                else ShowMenu();
                                return;
                            }
                        }
                        break;
                    case 3:
                        _isGuestMode = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Ви увійшли як Гість. Знижки недоступні.");
                        Console.ResetColor();
                        ShowGuestMenu();
                        return;
                    case 0:
                        Exit();
                        return;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Невірний вибір! Спробуйте ще раз.");
                        Console.ResetColor();
                        break;
                }
            } while (true);
        }

        static bool LoginUserAndRoute()
        {
            Console.WriteLine("\n=== ВХІД ===");
            Console.Write("Введіть email: ");
            string email = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Введіть пароль: ");
            string password = Console.ReadLine() ?? string.Empty;

            var users = _userService!.AllRead();
            var user = users.FirstOrDefault(u => u.Email != null && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null || user.HashedPassword == null || !PasswordHasher.VerifyPassword(password, user.HashedPassword))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Невірний email або пароль.");
                Console.ResetColor();
                return false;
            }
            
            _currentUser = user;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Вхід успішний! Ви увійшли як {_currentUser.Role}.");
            Console.ResetColor();

            if (_currentUser.Role == "Admin")
            {
                AdminShowMenu();
            }
            else
            {
                ShowMenu();
            }
            
            return true; 
        }

        static void LoginUserAfterRegistration()
        {
            var users = _userService!.AllRead();
            var user = users.LastOrDefault(); 

            if (user != null)
            {
                _currentUser = user;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Автоматичний вхід успішний як {_currentUser.Role}.");
                Console.ResetColor();
            }
        }

        static bool RegisterUser(string requestedRole) 
        {
            Console.WriteLine("\n=== РЕЄСТРАЦІЯ ===");
            Console.Write("Введіть email: ");
            string email = Console.ReadLine()?.Trim() ?? string.Empty;
            Console.Write("Введіть пароль: ");
            string password = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Email та пароль не можуть бути порожніми.");
                Console.ResetColor();
                return false;
            }

            var users = _userService!.AllRead();
            if (users.Any(u => u.Email != null && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Користувач з таким email вже зареєстрований.");
                Console.ResetColor();
                return false;
            }

            string hashedPassword = PasswordHasher.HashPassword(password);
            
            var newUser = new User
            {
                Id = 0,
                Email = email,
                HashedPassword = hashedPassword,
                Role = "User"
            };

            if (users.Count == 0)
            {
                 newUser.Role = "Admin";
            }
            else if (requestedRole == "Admin") 
            {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine("Помилка: Реєстрація нового адміністратора не дозволена.");
                 Console.ResetColor();
                 return false;
            }
            
            _userService.Add(newUser); 

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Реєстрація успішна! Ваша роль: {newUser.Role}.");
            Console.ResetColor();
            return true; 
        }

        static void AdminShowMenu()
        {
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

                if (!int.TryParse(Console.ReadLine(), out int choice))
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

        static void ShowGuestMenu() 
        {
            while (true)
            {
                Console.WriteLine("\n=== МЕНЮ ГОСТЯ ===");
                Console.WriteLine("1. Переглянути товари");
                Console.WriteLine("2. Купити товари");
                Console.WriteLine("0. Вийти");
                Console.Write("Ваш вибір: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Невірний вибір. Введіть число.");
                    continue;
                }

                switch (choice)
                {
                    case 1: ShowProducts(); break;
                    case 2: BuyProducts(); break;
                    case 0: return;
                    default: Console.WriteLine("Невірний вибір!"); break;
                }
            }
        }

        static void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== МЕНЮ ПОКУПЦЯ ===");
                Console.WriteLine("1. Переглянути товари");
                Console.WriteLine("2. Купити товари");
                Console.WriteLine("3. Статистика");
                Console.WriteLine("4. Звіт");
                Console.WriteLine("0. Вийти");
                Console.Write("Ваш вибір: ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
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
            var products = _productService!.AllRead();

            if (products.Count == 0)
            {
                Console.WriteLine("\nСписок товарів порожній.");
                return;
            }

            Console.WriteLine("\n------------------------------------------------");
            Console.WriteLine($"| {"ID",-5} | {"Назва",-25} | {"Ціна",10} |");
            Console.WriteLine("------------------------------------------------");
            foreach (var p in products)
            {
                Console.WriteLine($"| {p.Id,-5} | {p.Name,-25} | {p.Price,10:F2} |");
            }

            Console.WriteLine("------------------------------------------------");
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
                Console.Write($"Товар #{i + 1} Назва: ");
                string n = Console.ReadLine()!.Trim();

                if (string.IsNullOrWhiteSpace(n))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Назва товару не може бути порожньою. Спробуйте цей товар ще раз.");
                    Console.ResetColor();
                    i--;
                    continue;
                }

                Console.Write($"Товар #{i + 1} Ціна: ");

                if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double p) || p < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Некоректна ціна (повинна бути невід'ємним числом, використовуйте крапку). Спробуйте цей товар ще раз.");
                    Console.ResetColor();
                    i--;
                    continue;
                }
                
                _productService!.Add(new Product(id: 0, n, p));
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Успішно додано {c} товар(ів).");
            Console.ResetColor();
        }

        static void SearchProduct()
        {
            Console.Write("Введіть назву товару для пошуку: ");
            string key = Console.ReadLine()!.ToLower().Trim();

            var products = _productService!.AllRead();

            var results = products
                .Where(p => p.Name.ToLower().Contains(key))
                .ToList();

            Console.WriteLine("\n--- Результати пошуку ---");
            if (results.Any())
            {
                foreach (var p in results)
                {
                    Console.WriteLine($"Знайдено: {p.Name} — {p.Price:F2} грн (ID {p.Id})");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Не знайдено.");
                Console.ResetColor();
            }
        }

        static void DeleteProduct()
        {
            ShowProducts();
            var products = _productService!.AllRead();
            if (products.Count == 0) return;

            Console.Write("\nВведіть ID товару, який потрібно видалити: ");

            if (!int.TryParse(Console.ReadLine(), out int idToDelete))
            {
                Console.WriteLine("Невірний формат ID.");
                return;
            }

            var productToDelete = products.FirstOrDefault(p => p.Id == idToDelete);

            if (productToDelete != null)
            {
                var updatedProducts = products.Where(p => p.Id != idToDelete).ToList();

                _productService.GetType().GetMethod("ReindexAndWriteAll", BindingFlags.Instance | BindingFlags.NonPublic)?
                    .Invoke(_productService, new object[] { updatedProducts });
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Видалено товар: {productToDelete.Name} (старий ID {idToDelete})!");
                Console.WriteLine("Увага: ID всіх наступних товарів були оновлені.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Помилка: Товар з ID {idToDelete} не знайдено.");
                Console.ResetColor();
            }
        }

        static void SortByPriceBubble()
        {
            var products = _productService!.AllRead();
            int n = products.Count;

            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (products[j].Price > products[j + 1].Price)
                    {
                        var temp = products[j];
                        products[j] = products[j + 1];
                        products[j + 1] = temp;
                    }
                }
            }
            Console.WriteLine("Відсортовано за ціною (Бульбашкове сортування).");
            ShowProducts(products);
        }

        static void SortByNameBuiltin()
        {
            var products = _productService!.AllRead();

            products.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
            );
            Console.WriteLine("Відсортовано за назвою (Вбудований List.Sort).");
            ShowProducts(products);
        }

        static void ShowProducts(List<Product> products)
        {
            if (products.Count == 0) return;

            Console.WriteLine("\n--- Відсормований список ---");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"| {"ID",-5} | {"Назва",-25} | {"Ціна",10} |");
            Console.WriteLine("------------------------------------------------");
            foreach (var p in products)
            {
                Console.WriteLine($"| {p.Id,-5} | {p.Name,-25} | {p.Price,10:F2} |");
            }
            Console.WriteLine("------------------------------------------------");
        }


        static void ShowSortingMenu()
        {
            var products = _productService!.AllRead();
            if (products.Count < 2)
            {
                Console.WriteLine("Недостатньо елементів для сортування (потрібно мінімум 2).");
                return;
            }

            Console.WriteLine("\n=== МЕНЮ СОРТУВАННЯ ===");
            Console.WriteLine("1. За ціною (Бульбашкове)");
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
            var products = _productService!.AllRead();

            if (products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній. Статистика недоступна.");
                return;
            }

            double totalPrice = products.Sum(prod => prod.Price);
            double minPrice = products.Min(prod => prod.Price);
            double maxPrice = products.Max(prod => prod.Price);

            const double tolerance = 0.0001; 
            var minProduct = products.FirstOrDefault(prod => Math.Abs(prod.Price - minPrice) < tolerance);
            var maxProduct = products.FirstOrDefault(prod => Math.Abs(prod.Price - maxPrice) < tolerance);

            double average = products.Count > 0 ? totalPrice / products.Count : 0;

            Console.WriteLine("\n=== СТАТИСТИКА ТОВАРІВ ===");
            Console.WriteLine($"Кількість товарів: {products.Count}");
            Console.WriteLine($"Загальна сума цін: {totalPrice:F2} грн");
            Console.WriteLine($"Середня ціна: {Math.Round(average, 2):F2} грн");

            if (minProduct != null && maxProduct != null) 
            {
                Console.WriteLine($"Мінімальна ціна: {minProduct.Name} (ID {minProduct.Id}) — {minPrice:F2} грн");
                Console.WriteLine($"Максимальна ціна: {maxProduct.Name} (ID {maxProduct.Id}) — {maxPrice:F2} грн");
            }
        }

        static void BuyProducts()
        {
            var products = _productService!.AllRead();
            if (products.Count == 0)
            {
                Console.WriteLine("Список товарів порожній. Немає чого купувати.");
                return;
            }

            double sum = 0;
            Random rand = new Random();
            Console.WriteLine("\nДоступні товари:");
            ShowProducts();
            
            Console.WriteLine("Виберіть товари (Назва:Кількість, або 'стоп' для завершення):");
            string? input;

            var purchasedItems = new Dictionary<string, (int Qty, double Price)>();

            do
            {
                Console.Write("Введіть товар: ");
                input = Console.ReadLine();
                if (input == null || input.ToLower() == "стоп") break;

                var parts = input.Split(':');

                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || !int.TryParse(parts[1].Trim(), out int qty))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Невірний формат! Очікується 'Назва товару:Кількість'.");
                    Console.ResetColor();
                    continue;
                }

                string nameQuery = parts[0].Trim().ToLower();

                if (qty <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Кількість повинна бути більшою за нуль!");
                    Console.ResetColor();
                    continue;
                }

                var product = products.FirstOrDefault(p => p.Name.ToLower().Contains(nameQuery));

                if (product == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Товар, що містить '{nameQuery}', не знайдено.");
                    Console.ResetColor();
                    continue;
                }
                
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
                double discount = 0;

                if (!_isGuestMode) 
                {
                    discount = rand.Next(0, 21) / 100.0;
                }
                
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
                
                if (discount > 0)
                {
                    Console.WriteLine($"Знижка ({discount * 100:F0}%): -{Math.Round(sum * discount, 2):F2} грн");
                }
                else
                {
                    Console.WriteLine("Знижка: Не застосовано");
                }
                
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

