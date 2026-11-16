using System;
using System.Threading.Tasks;
using MyCryptoProject.lr1.Task1;
using MyCryptoProject.lr1.Task4;
using MyCryptoProject.lr1.Task6;
using MyCryptoProject.lr1.Task5;
using MyCryptoProject.lr1.Task7;
using MyCryptoProject.lr1.Tests;

namespace MyCryptoProject
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("   ЛАБОРАТОРНАЯ РАБОТА №1");
                Console.WriteLine("  1. Запуск Unit-тестов P-блока (Задание 1)");
                Console.WriteLine("  2. Комплексная демонстрация DES + Режимы + Бенчмарк (Задания 2-5)");
                Console.WriteLine("  3. Комплексная демонстрация DEAL + Адаптер DES (Задания 6-7)");
                Console.WriteLine("  4. Тестирование граничных случаев (Edge Cases: Нули, Пустые файлы, C++ код)");
                Console.WriteLine("  0. Выход из программы");
                Console.WriteLine("================================================================================");
                Console.Write("Ваш выбор: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        RunPBoxDemo();
                        break;

                    case "2":
                        await DesDemonstrator.RunAllDemosAsync();
                        break;

                    case "3":
                        await DealDemonstrator.RunAllDemosAsync();
                        break;

                    case "4":
                        await RunEdgeCasesMenuAsync();
                        break;

                    case "0":
                        Console.WriteLine("Выход из программы.");
                        return;

                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }

                Console.WriteLine("\nНажмите любую клавишу для возврата в главное меню...");
                Console.ReadKey();
            }
        }

        private static void RunPBoxDemo()
        {
            Console.WriteLine("=== ДЕМОНСТРАЦИЯ РАБОТЫ P-БЛОКА ===");
            byte[] input = new byte[] { 0xAD }; // 10101101
            int[] pBox = new int[] { 8, 7, 6, 5, 4, 3, 2, 1 };
            byte[] output = PermutationBox.Permute(input, pBox, baseIndex: 1, isMsb0: true);

            Console.WriteLine($"Вход:  0x{input[0]:X2} ({Convert.ToString(input[0], 2).PadLeft(8, '0')})");
            Console.WriteLine($"Выход: 0x{output[0]:X2} ({Convert.ToString(output[0], 2).PadLeft(8, '0')})");
            Console.WriteLine("\nЗапуск полного набора Unit-тестов для P-блока:\n");
            
            PermutationBoxTests.RunAllTests();
        }

        private static async Task RunEdgeCasesMenuAsync()
        {
            Console.WriteLine("Выберите алгоритм для проверки граничных случаев (Edge Cases):");
            Console.WriteLine("  1. Проверить на DES (блок 8 байт, ключ 8 байт)");
            Console.WriteLine("  2. Проверить на DEAL (блок 16 байт, ключ 16 байт)");
            Console.Write("Ваш выбор: ");

            string subChoice = Console.ReadLine();
            Console.WriteLine();

            if (subChoice == "1")
            {
                var des = new DesCipher();
                byte[] desKey = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] desIv  = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };
                await TestFileManager.RunPhysicalFileTestsAsync(des, desKey, desIv, "DES");
            }
            else if (subChoice == "2")
            {
                var desInstance = new DesCipher();
                var deal = new DealCipher(desInstance);
                byte[] dealKey = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }; // 16 байт
                byte[] dealIv  = new byte[] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 }; // 16 байт
                await TestFileManager.RunPhysicalFileTestsAsync(deal, dealKey, dealIv, "DEAL");
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
            }
        }
    }
}
