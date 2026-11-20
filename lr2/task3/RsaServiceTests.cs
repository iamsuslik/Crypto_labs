using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task3
{
    public static class RsaServiceTests
    {
        public static void RunTests()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("===     ДЕМОНСТРАЦИЯ И ТЕСТЫ TASK 3 (RSA СЕРВИС)       ===");
            Console.WriteLine("==========================================================\n");

            // Тест 1: Основной тест с тестом простоты Миллера-Рабина
            TestSingleRsaConfiguration(RsaService.PrimalityTestType.MillerRabin, "Миллер-Рабин", 128);

            // Тест 2: Проверка с тестом Ферма (для демонстрации универсальности)
            TestSingleRsaConfiguration(RsaService.PrimalityTestType.Fermat, "Ферма", 128);

            // Тест 3: Проверка с тестом Соловея-Штрассена
            TestSingleRsaConfiguration(RsaService.PrimalityTestType.SolovayStrassen, "Соловей-Штрассен", 128);

            Console.WriteLine("==========================================================");
            Console.WriteLine("===           ТЕСТЫ TASK 3 ЗАВЕРШЕНЫ                   ===");
            Console.WriteLine("==========================================================\n");
        }

        private static void TestSingleRsaConfiguration(RsaService.PrimalityTestType testType, string testName, int bitLength)
        {
            Console.WriteLine($"[*] --- Тестирование RSA с генерацией простых чисел ({testName}, {bitLength} бит) ---");

            // 1. Создание сервиса и генерация ключей
            var rsa = new RsaService(testType, minProbability: 0.99, bitLength: bitLength);
            rsa.GenerateKeys();

            // 2. Вывод сгенерированных параметров ключей
            Console.WriteLine("[+] Ключевая пара успешно создана:");
            Console.WriteLine($"    Модуль (N):              {rsa.PublicKey_N}");
            Console.WriteLine($"    Открытая экспонента (e): {rsa.PublicKey_E}");
            Console.WriteLine($"    Секретная экспонента (d):{rsa.PrivateKey_D}");

            // 3. Шифрование и расшифрование тестового сообщения
            BigInteger originalMessage = 42424242;
            Console.WriteLine($"\n    [>] Исходное сообщение (M):      {originalMessage}");

            BigInteger cipherText = rsa.Encrypt(originalMessage);
            Console.WriteLine($"    [>] Зашифрованное сообщение (C): {cipherText}");

            BigInteger decryptedMessage = rsa.Decrypt(cipherText);
            Console.WriteLine($"    [>] Расшифрованное сообщение (M'):{decryptedMessage}");

            // 4. Проверка корректности
            bool isSuccess = (originalMessage == decryptedMessage);
            PrintTestResult($"Шифрование и расшифрование RSA ({testName})", isSuccess);

            // 5. Граничный тест (маленькое число)
            BigInteger smallMessage = 7;
            bool isSmallSuccess = (rsa.Decrypt(rsa.Encrypt(smallMessage)) == smallMessage);
            PrintTestResult($"Граничный тест (сообщение = 7)", isSmallSuccess);

            Console.WriteLine();
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
