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
        Environment.Exit(0);
    }

    static void register()
    {
        do
        {
            Console.WriteLine("Ввійти як покупець чи адміністратор? (покупець, адміністратор)");
            string choice = Console.ReadLine().ToLower();
            if (choice == "покупець")
            {
                ShowMenu();
            }
            else if (choice == "адміністратор")
            {
                Login();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Невірний вибір! Спробуйте ще раз.");
                Console.ResetColor();
                continue;
            }
            break;
        } while (true);
    }

    static void Login()
    {
        int attempts = 0;
        string enteredLogin, enteredPassword;
        do
        {
            Console.Write("Введіть логін: ");
            enteredLogin = Console.ReadLine();
            Console.Write("Введіть пароль: ");
            enteredPassword = Console.ReadLine();

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
            Console.WriteLine("1. Переглянути товари");
            Console.WriteLine("2. Редагувати товари");
            Console.WriteLine("3. Звіт");
            Console.WriteLine("0. Вихід");
            Console.Write("Ваш вибір: ");
            if (!int.TryParse(Console.ReadLine(), out choice)) continue;

            switch (choice)
            {
                case 1: ShowProducts(); break;
                case 2: EditPurchase(); break;
                case 3: ShowReport(); break;
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
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("МЕНЮ МАГАЗИНУ");
            Console.ResetColor();
            Console.WriteLine("1. Переглянути товари");
            Console.WriteLine("2. Купити товари");
            Console.WriteLine("3. Інформація про магазин");
            Console.WriteLine("4. Звіт");
            Console.WriteLine("0. Вихід");
            Console.Write("Ваш вибір: ");

            if (!int.TryParse(Console.ReadLine(), out choice)) continue;

            switch (choice)
            {
                case 1: ShowProducts(); break;
                case 2: CalculatePurchase(); break;
                case 3: ShowInfo(); break;
                case 4: ShowReport(); break;
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
                string json = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonSerializer.Deserialize<List<Product>>(json);
                    products = loaded?.Where(p => p != null).ToList() ?? new List<Product>();
                }
            }
            catch
            {
                Console.WriteLine("Помилка завантаження. Використовуємо дефолтні.");
            }
        }
        if (products.Count == 0) GetDefaultProducts();
        SaveProducts();
    }

    static void GetDefaultProducts()
    {
        products.Clear();
        string[] names = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
        double[] prices = { 500, 300, 700, 150, 250 };

        for (int i = 0; i < 5; i++)
        {
            products.Add(new Product(names[i], prices[i]));
        }
        SaveProducts();
    }

    static void SaveProducts()
    {
        string json = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    static void EditPurchase()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Додати новий товар чи змінити існуючий? (додати / змінити)");
        Console.ResetColor();
        string choice1 = Console.ReadLine().ToLower().Trim();

        if (choice1 == "додати")
        {
            Console.Write("Назва: "); string name = Console.ReadLine().Trim();
            if (string.IsNullOrWhiteSpace(name) || products.Any(p => p != null && p.name?.ToLower() == name.ToLower())) { Console.WriteLine("Невірна назва."); return; }
            Console.Write("Ціна: "); if (!double.TryParse(Console.ReadLine(), out double price) || price < 0) { Console.WriteLine("Невірна ціна."); return; }
            products.Add(new Product(name, price));
            SaveProducts();
            Console.WriteLine("Додано!");
        }
        else if (choice1 == "змінити")
        {
            ShowProducts();
            Console.Write("Назва для зміни: "); string editname = Console.ReadLine().ToLower().Trim();
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i] != null && products[i].name?.ToLower() == editname)
                {
                    Console.Write("Нова назва: "); products[i].name = Console.ReadLine().Trim();
                    Console.Write("Нова ціна: "); 
                    if (double.TryParse(Console.ReadLine(), out double newPrice)) products[i].price = newPrice;
                    SaveProducts();
                    Console.WriteLine("Змінено!");
                    break;
                }
            }
            if (products.All(p => p == null || p.name?.ToLower() != editname)) Console.WriteLine("Не знайдено!");
        }
    }

    static void ShowProducts()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Перелік товарів");
        Console.ResetColor();
        foreach (var p in products) if (!string.IsNullOrEmpty(p.name)) ShowInfo(p);
    }

    static void ShowInfo(Product p)
    {
        Console.WriteLine($"Товар: {p.name}, Ціна: {p.price} грн");
    }

    static void CalculatePurchase()
    {
        double sum = 0;
        foreach (var p in products.Where(p => !string.IsNullOrEmpty(p.name)))
        {
            ShowInfo(p);
            Console.Write($"Кількість '{p.name}' (шт.): ");
            if (double.TryParse(Console.ReadLine(), out double qty) && qty > 0)
            {
                sum += p.price * qty;
            }
            else continue;
        }
        CalculateDiscount(sum, out double total);
        Console.WriteLine($"Загальна вартість зі знижкою: {Math.Round(total, 2)} грн");
        Console.WriteLine("Показати статистику? (так/ні)");
        if (Console.ReadLine().ToLower() == "так") ShowStatistics();
    }

    static void ShowReport()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=== ЗВІТ МАГАЗИНУ ===");
        Console.ResetColor();
        Console.WriteLine("Перелік товарів:");
        Console.WriteLine(new string('-', 40));
        foreach (var p in products.Where(p => !string.IsNullOrEmpty(p.name)))
        {
            Console.WriteLine($"| {p.name,-25} | {p.price,8:F2} грн |");
        }
        Console.WriteLine(new string('-', 40));
        Console.WriteLine("\nПідсумкові дані:");
        double totalPrice = products.Where(p => !string.IsNullOrEmpty(p.name) && p.price > 0).Sum(p => p.price);
        double average = products.Count(p => !string.IsNullOrEmpty(p.name) && p.price > 0) > 0 ? totalPrice / products.Count(p => !string.IsNullOrEmpty(p.name) && p.price > 0) : 0;
        int countAbove500 = products.Count(p => !string.IsNullOrEmpty(p.name) && p.price > 500);
        double minPrice = products.Where(p => !string.IsNullOrEmpty(p.name) && p.price > 0).Min(p => p.price);
        double maxPrice = products.Where(p => !string.IsNullOrEmpty(p.name) && p.price > 0).Max(p => p.price);
        Console.WriteLine($"Загальна сума: {totalPrice:F2} грн");
        Console.WriteLine($"Середня ціна: {average:F2} грн");
        Console.WriteLine($"Товарів >500 грн: {countAbove500}");
        Console.WriteLine($"Мін ціна: {minPrice:F2} грн");
        Console.WriteLine($"Макс ціна: {maxPrice:F2} грн");
        Console.WriteLine("=== КІНЕЦЬ ЗВІТУ ===");
    }

    static void ShowInfo()
    {
        Console.WriteLine("Магазин подарунків 'HappyBox'");
        Console.WriteLine("Місто: Іршава");
        Console.WriteLine("Режим роботи: 9:00 – 19:00");
    }

    static void CalculateDiscount(double total, out double result)
    {
        double discount = new Random().Next(5, 15);
        result = total * (1 - discount / 100);
        Console.WriteLine($"Знижка: {discount}%");
    }

    static void ShowStatistics()
    {
        double totalPrice = 0, sum = 0;
        int count = 0, countAbove500 = 0;
        double minPrice = double.MaxValue, maxPrice = double.MinValue;
        string minName = "", maxName = "";

        for (int i = 0; i < products.Count; i++)
        {
            var p = products[i];
            if (string.IsNullOrEmpty(p.name) || p.price <= 0) continue;

            totalPrice += p.price;
            sum += p.price;
            count++;
            if (p.price > 500) countAbove500++;

            if (p.price < minPrice) { minPrice = p.price; minName = p.name; }
            if (p.price > maxPrice) { maxPrice = p.price; maxName = p.name; }
        }

        double average = count > 0 ? sum / count : 0;
        Console.WriteLine("Статистика:");
        Console.WriteLine($"Загальна сума: {totalPrice} грн");
        Console.WriteLine($"Середня ціна: {average:F2} грн");
        Console.WriteLine($"Товарів >500 грн: {countAbove500}");
        Console.WriteLine($"Мін: {minName} ({minPrice} грн)");
        Console.WriteLine($"Макс: {maxName} ({maxPrice} грн)");
    }
}