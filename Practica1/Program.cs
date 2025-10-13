using System;

class Program
{
    static void Main()
    {
        double price1 = 500.00;
        double price2 = 300.00;
        double price3 = 700.00;
        double prise4 = 150.00;
        double prise5 = 250.00;

        string product1 = "Подарунковий набір";
        string product2 = "Імена чашка";
        string product3 = "Букет";
        string product4 = "Аромо свічка";
        string product5 = "Шоколодний набір";

        int unit1, unit2, unit3, unit4, unit5;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Потрібно " + product1 + " (шт.)"); 
        unit1 = Convert.ToInt32(Console.ReadLine()); 
        Console.WriteLine("Потрібно " + product2 + " (шт.)"); 
        unit2 = Convert.ToInt32(Console.ReadLine()); 
        Console.WriteLine("Потрібно " + product3 + " (шт.)"); 
        unit3 = Convert.ToInt32(Console.ReadLine()); 
        Console.WriteLine("Потрібно " + product4 + " (шт.)"); 
        unit4 = Convert.ToInt32(Console.ReadLine()); 
        Console.WriteLine("Потрібно " + product5 + " (шт.)"); 
        unit5 = Convert.ToInt32(Console.ReadLine());
        Console.ResetColor();

        double cost1 = price1 * unit1;
        double cost2 = price2 * unit2;
        double cost3 = price3 * unit3;
        double cost4 = prise4 * unit4;
        double cost5 = prise5 * unit5;  

        double total = cost1 + cost2 + cost3 + cost4 + cost5;

        total = Math.Round(total, 2);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Деталізація замовлення:");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Black;
        Console.WriteLine("Вартість " + product1 + " за одиницю "  + price1 +  ". Загальна: " + cost1 + " грн.");
        Console.WriteLine("Вартість " + product2 + " за одиницю "  + price2 +  ". Загальна: " + cost2 + " грн.");
        Console.WriteLine("Вартість " + product3 + " за одиницю "  + price3 +  ". Загальна: " + cost3 + " грн."); 
        Console.WriteLine("Вартість " + product4 + " за одиницю "  + prise4 +  ". Загальна: " + cost4 + " грн.");
        Console.WriteLine("Вартість " + product5 + " за одиницю "  + prise5 +  ". Загальна: " + cost5 + " грн.");   
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Загальна вартість замовлення: " + total + " грн.");

        Console.ReadKey();
    }
}