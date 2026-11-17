using System;

namespace MyCryptoProject.lr1.Task1
{
    public static class PermutationBoxTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== ЗАПУСК UNIT-ТЕСТОВ ДЛЯ PermutationBox ===\n");

            Test_IdentityPermutation();
            Test_ReversePermutation();
            Test_ExpansionPBox_32to48();
            Test_CompressionPBox_64to56();
            Test_ZeroBasedIndexing();
            Test_NullInput_ThrowsException();

            Console.WriteLine("\n=== ВСЕ ТЕСТЫ УСПЕШНО ПРОЙДЕНЫ! ===");
        }

        // 1. Тест: Тождественная перестановка (выход должен совпадать с входом)
        private static void Test_IdentityPermutation()
        {
            byte[] input = new byte[] { 0xAB }; // 10101011
            int[] pBox = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            byte[] result = PermutationBox.Permute(input, pBox, baseIndex: 1, isMsb0: true);

            Assert(input[0] == result[0], "Test_IdentityPermutation");
        }

        // 2. Тест: Разворот битов байта задом наперед
        private static void Test_ReversePermutation()
        {
            byte[] input = new byte[] { 0x80 }; // 10000000 (MSB 0: единица на первой позиции)
            int[] pBox = new int[] { 8, 7, 6, 5, 4, 3, 2, 1 };

            byte[] result = PermutationBox.Permute(input, pBox, baseIndex: 1, isMsb0: true);

            // После инверсии единица должна оказаться на последней позиции (00000001 = 0x01)
            Assert(result[0] == 0x01, "Test_ReversePermutation");
        }

        // 3. Тест: Расширяющая перестановка (32 бит -> 48 бит, как E-box в DES)
        private static void Test_ExpansionPBox_32to48()
        {
            byte[] input = new byte[4]; // 32 бита
            input[0] = 0x80; // Первый бит равен 1

            // Таблица из 48 элементов (дублирует биты)
            int[] expansionPBox = new int[48];
            for (int i = 0; i < 48; i++)
            {
                expansionPBox[i] = 1; // Заполняем всеми единичками для проверки дублирования 1-го бита
            }

            byte[] result = PermutationBox.Permute(input, expansionPBox, baseIndex: 1, isMsb0: true);

            // Результат должен быть размером 6 байт (48 бит), и все биты должны быть 1
            Assert(result.Length == 6, "Test_ExpansionPBox_Length");
            Assert(result[0] == 0xFF && result[5] == 0xFF, "Test_ExpansionPBox_Values");
        }

        // 4. Тест: Сжимающая перестановка (64 бит -> 56 бит, как PC-1 в DES)
        private static void Test_CompressionPBox_64to56()
        {
            byte[] input = new byte[8]; // 64 бита (8 байт)
            int[] pc1PBox = new int[56];

            for (int i = 0; i < 56; i++)
            {
                pc1PBox[i] = i + 1;
            }

            byte[] result = PermutationBox.Permute(input, pc1PBox, baseIndex: 1, isMsb0: true);

            Assert(result.Length == 7, "Test_CompressionPBox_Length (56 бит = 7 байт)");
        }

        // 5. Тест: Проверка работы с 0-based индексацией
        private static void Test_ZeroBasedIndexing()
        {
            byte[] input = new byte[] { 0x80 }; // 10000000
            int[] pBox = new int[] { 7, 6, 5, 4, 3, 2, 1, 0 }; // 0-based индексация

            byte[] result = PermutationBox.Permute(input, pBox, baseIndex: 0, isMsb0: true);

            Assert(result[0] == 0x01, "Test_ZeroBasedIndexing");
        }

        // 6. Тест: Проверка выброса исключения при null
        private static void Test_NullInput_ThrowsException()
        {
            try
            {
                PermutationBox.Permute(null!, new int[] { 1 });
                Assert(false, "Test_NullInput_ThrowsException (должно было выбросить исключение)");
            }
            catch (ArgumentNullException)
            {
                Assert(true, "Test_NullInput_ThrowsException");
            }
        }

        // Вспомогательный метод для проверки условий
        private static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                Console.WriteLine($"[ПРОЙДЕН] {testName}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ОШИБКА] {testName}");
                Console.ResetColor();
                throw new Exception($"Тест {testName} не пройден!");
            }
        }
    }
}
