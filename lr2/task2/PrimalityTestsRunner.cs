using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task2
{
    public static class PrimalityTestsRunner
    {
        public static void RunTests()
        {
            Console.WriteLine("=== АВТОТЕСТЫ ДЛЯ TASK 2 (Тесты простоты) ===");

            IPrimalityTest fermat = new FermatTest();
            IPrimalityTest solovay = new SolovayStrassenTest();
            IPrimalityTest millerRabin = new MillerRabinTest();

            BigInteger prime = 104729;
            BigInteger composite = 561;

            // Тест Ферма
            PrintTestResult("Fermat Test on Prime (104729)", fermat.IsPrime(prime, 0.99));
            // Число 561 — классический обманщик Ферма, но проверим его Миллером-Рабином:

            // Миллер-Рабин
            PrintTestResult("Miller-Rabin Test on Prime (104729)", millerRabin.IsPrime(prime, 0.99));
            PrintTestResult("Miller-Rabin Test on Composite (561)", !millerRabin.IsPrime(composite, 0.99));

            // Соловей-Штрассен
            PrintTestResult("Solovay-Strassen Test on Prime (104729)", solovay.IsPrime(prime, 0.99));
            PrintTestResult("Solovay-Strassen Test on Composite (561)", !solovay.IsPrime(composite, 0.99));

            Console.WriteLine("=== ВСЕ ТЕСТЫ TASK 2 ЗАВЕРШЕНЫ ===\n");
        }

        private static void PrintTestResult(string testName, bool success)
        {
            Console.Write($"   [{ (success ? "ПРОЙДЕН" : "ОШИБКА") }] {testName}\n");
        }
    }
}
