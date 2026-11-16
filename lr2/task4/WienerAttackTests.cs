using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task4
{
    public static class WienerAttackTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== АВТОТЕСТЫ ДЛЯ TASK 4 (Атака Винера) ===");

            var vulnPair = VulnerableRsaGenerator.GenerateVulnerablePair(256);
            
            var wiener = new WienerAttackService();
            var result = wiener.Attack(vulnPair.e, vulnPair.n);

            PrintTestResult("Wiener Attack successfully cracked vulnerable RSA key", result.Success && result.PrivateKeyD == vulnPair.d);

            Console.WriteLine("=== ВСЕ ТЕСТЫ TASK 4 ЗАВЕРШЕНЫ ===\n");
        }

        private static void PrintTestResult(string testName, bool success)
        {
            Console.Write($"   [{ (success ? "ПРОЙДЕН" : "ОШИБКА") }] {testName}\n");
        }
    }
}
