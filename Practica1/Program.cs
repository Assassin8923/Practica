using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Program
{
    class Product
    {
        public string name { get; set; } = "";
        public double price { get; set; } = 0;
        public Product(string name, double price)
        {
            this.name = name;
            this.price = price;
        }
    }
    static List<Product> products = new();
    static string filePath = "products.json";
    static void Main()
    {
        LoadProducts();
        register();
    }
    static void exit()
    {
        SaveProducts();
        Environment.Exit(0);
    }
    static void register()
    {
        do
        {
            Console.WriteLine("Ввійти як покупець чи адміністратор? (покупець, адміністратор)");
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
                adminShowMenu();
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
        exit();
    }
    static void adminShowMenu()
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
                case 5: SortProducts(); break;
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
        if (File.Exists(filePath))
        {
            try
            {
                var loaded = JsonSerializer.Deserialize<List<Product>>(File.ReadAllText(filePath));
                if (loaded != null) products = loaded.Where(p => p != null).ToList();
            }
            catch { }
        }
        if (products.Count == 0)
        {
            string[] names = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
            double[] prices = { 500, 300, 700, 150, 250 };
            for (int i = 0; i < 5; i++)
                products.Add(new Product(names[i], prices[i]));
            SaveProducts();
        }
    }
    static void SaveProducts()
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true }));
    }
    static void ShowProducts()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("Список товарів порожній.");
            return;
        }
        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine($"| {"Індекс",-5} | {"Назва",-25} | {"Ціна",10} |");
        Console.WriteLine("-------------------------------------------");
        for (int i = 0; i < products.Count; i++)
        {
            var p = products[i];
            Console.WriteLine($"| {i,-5} | {p.name,-25} | {p.price,10:F2} |");
        }
        Console.WriteLine("-------------------------------------------");
    }
    static void AddProducts()
    {
        Console.Write("Скільки товарів додати: ");
        if (!int.TryParse(Console.ReadLine(), out int c) || c <= 0) return;
        for (int i = 0; i < c; i++)
        {
            Console.Write("Назва: ");
            string n = Console.ReadLine()!;
            Console.Write("Ціна: ");
            if (!double.TryParse(Console.ReadLine(), out double p) || p < 0) { i--; continue; }
            products.Add(new Product(n, p));
        }
        SaveProducts();
    }
    static void SearchProduct()
    {
        Console.Write("Введіть назву товару для пошуку: ");
        string key = Console.ReadLine()!.ToLower();
        for (int i = 0; i < products.Count; i++)
        {
            if (products[i].name.ToLower() == key)
            {
                Console.WriteLine($"Знайдено: {products[i].name} — {products[i].price} грн (Індекс {i})");
                return;
            }
        }
        Console.WriteLine("Не знайдено.");
    }
    static void DeleteProduct()
    {
        Console.WriteLine("1. За індексом");
        Console.WriteLine("2. За назвою");
        Console.Write("Вибір: ");
        if (!int.TryParse(Console.ReadLine(), out int c)) return;
        if (c == 1)
        {
            Console.Write("Введіть індекс: ");
            if (!int.TryParse(Console.ReadLine(), out int id)) return;
            if (id >= 0 && id < products.Count)
            {
                products.RemoveAt(id);
                Console.WriteLine("Видалено!");
            }
        }
        else if (c == 2)
        {
            Console.Write("Введіть назву: ");
            string n = Console.ReadLine()!.ToLower();
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].name.ToLower() == n)
                {
                    products.RemoveAt(i);
                    Console.WriteLine("Видалено!");
                    break;
                }
            }
        }
        SaveProducts();
    }
    static void SortProducts()
    {
        Console.WriteLine("1. За ціною");
        Console.WriteLine("2. За назвою");
        Console.Write("Вибір: ");
        if (!int.TryParse(Console.ReadLine(), out int c)) return;
        if (c == 1) products.Sort((a, b) => a.price.CompareTo(b.price));
        else if (c == 2) products.Sort((a, b) => a.name.CompareTo(b.name));
        SaveProducts();
    }
    static void ShowStatistics()
    {
        if (products.Count == 0) return;
        var validProducts = products.Where(p => p.price > 0).ToList();
        if (validProducts.Count == 0) return;
        double totalPrice = validProducts.Sum(p => p.price);
        double average = totalPrice / validProducts.Count;
        double minPrice = validProducts.Min(p => p.price);
        double maxPrice = validProducts.Max(p => p.price);
        int countAbove500 = validProducts.Count(p => p.price > 500);
        var minProduct = validProducts.First(p => p.price == minPrice);
        var maxProduct = validProducts.First(p => p.price == maxPrice);
        Console.WriteLine($"\nКількість: {validProducts.Count}");
        Console.WriteLine($"Сума: {totalPrice:F2}");
        Console.WriteLine($"Середня ціна: {Math.Round(average, 2):F2}");
        Console.WriteLine($"Мінімум: {minProduct.name} — {minPrice:F2}");
        Console.WriteLine($"Максимум: {maxProduct.name} — {maxPrice:F2}");
        Console.WriteLine($"Кількість >500 грн: {countAbove500}");
    }
    static void BuyProducts()
    {
        if (products.Count == 0)
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
            if (idx < 0 || idx >= products.Count || qty <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Невірний індекс або кількість!");
                Console.ResetColor();
                continue;
            }
            sum += products[idx].price * qty;
        } while (true);
        if (sum > 0)
        {
            double discount = rand.Next(0, 21) / 100.0;
            double total = sum * (1 - discount);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Сума: {Math.Round(sum, 2):F2} грн");
            Console.WriteLine($"Знижка: {discount * 100}%");
            Console.WriteLine($"До сплати: {Math.Round(total, 2):F2} грн");
            Console.ResetColor();
        }
    }
    static void GenerateReport()
    {
        Console.WriteLine("\n=== ЗВІТ МАГАЗИНУ ===");       
        ShowProducts();
        ShowStatistics();
    }
}