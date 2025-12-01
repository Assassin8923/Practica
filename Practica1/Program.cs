using System;
using System.Collections.Generic;
using System.IO;
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
            string choice = Console.ReadLine()!.ToLower();
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
            enteredLogin = Console.ReadLine()!;
            Console.Write("Введіть пароль: ");
            enteredPassword = Console.ReadLine()!;

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

    static void LoadProducts()
    {
        if (File.Exists(filePath))
        {
            try
            {
                var loaded = JsonSerializer.Deserialize<List<Product>>(File.ReadAllText(filePath));
                if (loaded != null) products = loaded;
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
        Console.WriteLine("2.  назвою");
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

        double sum = 0, min = products[0].price, max = products[0].price;
        string minN = products[0].name, maxN = products[0].name;

        for (int i = 0; i < products.Count; i++)
        {
            double p = products[i].price;
            sum += p;
            if (p < min) { min = p; minN = products[i].name; }
            if (p > max) { max = p; maxN = products[i].name; }
        }

        double avg = sum / products.Count;

        Console.WriteLine($"\nКількість: {products.Count}");
        Console.WriteLine($"Сума: {sum:F2}");
        Console.WriteLine($"Середня ціна: {avg:F2}");
        Console.WriteLine($"Мінімум: {minN} — {min:F2}");
        Console.WriteLine($"Максимум: {maxN} — {max:F2}");
    }

    static void ShowMenu() { }
}
