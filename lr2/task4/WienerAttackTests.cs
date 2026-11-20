using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task4
{
    public static class WienerAttackTests
    {
        public static void RunTests()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("===   ДЕМОНСТРАЦИЯ И ТЕСТЫ TASK 4 (АТАКА ВИНЕРА)       ===");
            Console.WriteLine("==========================================================\n");

            // 1. Генерация уязвимой пары (e, n) и секретного d
            Console.WriteLine("[*] Шаг 1: Генерация уязвимой ключевой пары RSA...");
            var vulnPair = VulnerableRsaGenerator.GenerateVulnerablePair(256);

            Console.WriteLine("    [+] Исходные параметры ключа:");
            Console.WriteLine($"        Модуль (N):              {vulnPair.n}");
            Console.WriteLine($"        Открытая экспонента (e): {vulnPair.e}");
            Console.WriteLine($"        Секретный ключ (d):      {vulnPair.d}");
            Console.WriteLine();

            // 2. Выполнение атаки Винера
            Console.WriteLine("[*] Шаг 2: Выполнение атаки Винера (разложение e/N в цепную дробь)...");
            var wiener = new WienerAttackService();
            var result = wiener.Attack(vulnPair.e, vulnPair.n);

            // 3. Вывод результатов, строго требуемых по заданию
            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[+] АТАКА ВИНЕРА УСПЕШНО ЗАВЕРШЕНА!");
                Console.ResetColor();

                Console.WriteLine("\n--- НАЙДЕННЫЕ ЗНАЧЕНИЯ ---");
                Console.WriteLine($" 1. Дешифрующая экспонента (d):  {result.PrivateKeyD}");
                Console.WriteLine($" 2. Значение функции Эйлера phi: {result.EulerPhi}");

                Console.WriteLine($"\n 3. Коллекция подходящих дробей для e/N (найдено шагов: {result.Convergents.Count}):");
                for (int i = 0; i < result.Convergents.Count; i++)
                {
                    var frac = result.Convergents[i];
                    bool isTarget = (frac.Denominator == result.PrivateKeyD);
                    string marker = isTarget ? " <--- [НАЙДЕННЫЙ КЛЮЧ: k/d]" : "";
                    
                    Console.WriteLine($"    [{i + 1,2}] k/d = {frac.Numerator} / {frac.Denominator}{marker}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[-] Ошибка: Атака Винера не смогла подобрать ключ.");
                Console.ResetColor();
            }

            Console.WriteLine();

            // 4. Проверка корректности (автотест)
            bool isSuccess = result.Success && (result.PrivateKeyD == vulnPair.d);
            PrintTestResult("Совпадение найденного d с оригинальным ключом", isSuccess);

            Console.WriteLine("\n==========================================================");
            Console.WriteLine("===           ТЕСТЫ TASK 4 ЗАВЕРШЕНЫ                   ===");
            Console.WriteLine("==========================================================\n");
        }

        private static void PrintTestResult(string testName, bool success)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write($"   [{(success ? "ПРОЙДЕН" : "ОШИБКА")}] ");
            Console.ForegroundColor = prevColor;
            Console.WriteLine(testName);
        }
    }
}
