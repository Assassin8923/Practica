using System;

class Program
{
    static void Main()
    {
        ShowMenu(); 
    }

    static void ShowMenu()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("МЕНЮ МАГАЗИНУ");
        Console.ResetColor();
        Console.WriteLine("1. Переглянути товари");
        Console.WriteLine("2. Розрахувати покупку");
        Console.WriteLine("3. Інформація про магазин");
        Console.WriteLine("0. Вихід");
        Console.Write("Ваш вибір: ");

        try
        {
            int choice = Convert.ToInt32(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    ShowProducts();
                    break;
                case 2:
                    CalculatePurchase();
                    break;
                case 3:
                    ShowInfo();
                    break;
                case 0:
                    Console.WriteLine("Вихід з програми...");
                    return;
                default:
                    Console.WriteLine("Невірний вибір!");
                    break;
            }
        }
        catch (FormatException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Помилка: введіть число!");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непередбачена помилка: {ex.Message}");
        }

        ShowMenu();
    }

    static void ShowProducts()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Перелік товарів");
        Console.ResetColor();

        string[] products = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
        double[] prices = { 500, 300, 700, 150, 250 };

        for (int i = 0; i < products.Length; i++)
        {
            ShowInfo(products[i], prices[i]);
        }
    }

    static void CalculatePurchase()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Розрахунок покупки");
        Console.ResetColor();

        string[] products = { "Подарунковий набір", "Іменна чашка", "Букет", "Аромо свічка", "Шоколадний набір" };
        double[] prices = { 500, 300, 700, 150, 250 };
        double sum = 0;

        for (int i = 0; i < products.Length; i++)
        {
            Console.Write($"Кількість '{products[i]}' (шт.): ");
            double qty;
            try
            {
                qty = Convert.ToDouble(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Некоректне введення, пропускаємо товар...");
                continue;
            }

            GetTotal(ref sum, prices[i] * qty);
        }

        CalculateDiscount(sum, out double total);
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"\nЗагальна вартість зі знижкою: {Math.Round(total, 2)} грн");
        Console.ResetColor();
    }

    static void ShowInfo()
    {
        Console.WriteLine("Інформація про магазин");
        Console.WriteLine("Магазин подарунків 'HappyBox'");
        Console.WriteLine("Місто: Іршава");
        Console.WriteLine("Режим роботи: 9:00 – 19:00");
    }


    static void GetTotal(ref double sum, double price)
    {
        sum += price;
    }

    static void CalculateDiscount(double total, out double result)
    {
        double discount = new Random().Next(5, 15);
        result = total * (1 - discount / 100);
        Console.WriteLine($"Знижка: {discount}%");
    }


    static void ShowInfo(string name)
    {
        Console.WriteLine($"Назва товару: {name}");
    }

    static void ShowInfo(string name, double price)
    {
        Console.WriteLine($"Товар: {name}, Ціна: {price} грн");
    }
}
