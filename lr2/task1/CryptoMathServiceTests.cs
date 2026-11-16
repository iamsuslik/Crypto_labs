using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task1
{
    public static class CryptoMathServiceTests
    {
        public static void RunTests()
        {
            Console.WriteLine("=== АВТОТЕСТЫ ДЛЯ TASK 1 (CryptoMathService) ===");

            BigInteger modPow = CryptoMathService.ManualModPow(3, 4, 5);
            PrintTestResult("ManualModPow (3^4 mod 5 == 1)", modPow == 1);

            int leg = CryptoMathService.LegendreSymbol(2, 7);
            PrintTestResult("LegendreSymbol (2 / 7 == 1)", leg == 1);

            int jac = CryptoMathService.JacobiSymbol(2, 15);
            PrintTestResult("JacobiSymbol (2 / 15)", jac == 1);

            BigInteger gcd = CryptoMathService.GcdEuclid(54, 24);
            PrintTestResult("GcdEuclid (54, 24 == 6)", gcd == 6);

            var ext = CryptoMathService.ExtendedGcd(30, 20);
            PrintTestResult("ExtendedGcd (30, 20 -> gcd=10)", ext.gcd == 10 && 30 * ext.x + 20 * ext.y == 10);

            BigInteger binGcd = CryptoMathService.BinaryGcd(54, 24);
            PrintTestResult("BinaryGcd (54, 24 == 6)", binGcd == 6);

            BigInteger phiDef = CryptoMathService.EulerPhiDefinition(10);
            BigInteger phiFact = CryptoMathService.EulerPhiFactorization(10);
            BigInteger phiFour = CryptoMathService.EulerPhiFourier(10);
            PrintTestResult("EulerPhi (Definition == 4)", phiDef == 4);
            PrintTestResult("EulerPhi (Factorization == 4)", phiFact == 4);
            PrintTestResult("EulerPhi (Fourier == 4)", phiFour == 4);

            BigInteger carm = CryptoMathService.CarmichaelLambda(8); 
            PrintTestResult("CarmichaelLambda (8 == 2)", carm == 2);

            Console.WriteLine("=== ВСЕ ТЕСТЫ TASK 1 ЗАВЕРШЕНЫ ===\n");
        }

        private static void PrintTestResult(string testName, bool success)
        {
            Console.Write($"   [{ (success ? "ПРОЙДЕН" : "ОШИБКА") }] {testName}\n");
        }
    }
}
