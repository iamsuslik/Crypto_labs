using System;
using MyCryptoProject.lr2.task1;
using MyCryptoProject.lr2.task2;
using MyCryptoProject.lr2.task3;
using MyCryptoProject.lr2.task4;

namespace MyCryptoProject.lr2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("================================================================================");
            RunAllTests();
        }

        private static void RunAllTests()
        {
            CryptoMathServiceTests.RunTests();
            PrimalityTestsRunner.RunTests();
            RsaServiceTests.RunTests();
            WienerAttackTests.RunTests();
        }
    }
}

