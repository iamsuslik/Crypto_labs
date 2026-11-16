using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task3
{
    public static class RsaServiceTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== АВТОТЕСТЫ ДЛЯ TASK 3 (RSA Сервис) ===");

            // Инициализируем RSA с тестом Миллера-Рабина
            var rsa = new RsaService(RsaService.PrimalityTestType.MillerRabin, 0.99, 128);
            rsa.GenerateKeys();

            BigInteger message = 4242;
            BigInteger encrypted = rsa.Encrypt(message);
            BigInteger decrypted = rsa.Decrypt(encrypted);

            PrintTestResult("RSA Encryption & Decryption (message == decrypted)", message == decrypted);

            Console.WriteLine("=== ВСЕ ТЕСТЫ TASK 3 ЗАВЕРШЕНЫ ===\n");
        }

        private static void PrintTestResult(string testName, bool success)
        {
            Console.Write($"   [{ (success ? "ПРОЙДЕН" : "ОШИБКА") }] {testName}\n");
        }
    }
}
