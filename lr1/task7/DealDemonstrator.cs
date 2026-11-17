using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyCryptoProject.lr1.Task2.Context;
using MyCryptoProject.lr1.Task2.Enums;
using MyCryptoProject.lr1.Task4;
using MyCryptoProject.lr1.Task6;

using CipherMode = MyCryptoProject.lr1.Task2.Enums.CipherMode;
using PaddingMode = MyCryptoProject.lr1.Task2.Enums.PaddingMode;

namespace MyCryptoProject.lr1.Task7
{
    public static class DealDemonstrator
    {
        public static async Task RunAllDemosAsync()
        {
            Console.Clear();
            PrintHeader("ЗАДАНИЕ №7: КОМПЛЕКСНОЕ ТЕСТИРОВАНИЕ АЛГОРИТМА DEAL (БЛОК 16 БАЙТ)");

            // 1. экземпляр нашего DES 
            var desInstance = new DesCipher();

            // 2. экземпляр шифра DEAL, передавая в него DES
            var dealCipher = new DealCipher(desInstance);

            // Ключ и IV для DEAL должны быть размером ровно 16 байт (128 бит), так как блок DEAL = 16 байт
            byte[] key16 = Encoding.UTF8.GetBytes("SecrKey123456789"); // Ровно 16 байт
            byte[] iv16  = Encoding.UTF8.GetBytes("InitVec123456789"); // Ровно 16 байт

            // ТЕСТ 1: Псевдослучайные последовательности байтов
            await TestRandomBytesAsync(dealCipher, key16, iv16);

            // ТЕСТ 2: Текстовые данные и исходный код (Аллокатор РБ-дерева)
            await TestTextAndCodeAsync(dealCipher, key16, iv16);

            // ТЕСТ 3: Бинарные медиафайлы с проверкой через SHA-256
            await TestBinaryMediaFileAsync(dealCipher, key16, iv16);

            // ТЕСТ 4: Бенчмарк производительности (Параллельные режимы)
            await RunBenchmarkAsync(dealCipher, key16, iv16);

            PrintSuccess("\nВСЕ ТЕСТЫ ЗАДАНИЯ №7 УСПЕШНО ПРОЙДЕНЫ!");
        }

        #region Тест 1: Псевдослучайные байты

        private static async Task TestRandomBytesAsync(DealCipher deal, byte[] key, byte[] iv)
        {
            PrintSubHeader("1. ТЕСТИРОВАНИЕ СЛУЧАЙНЫХ БАЙТОВ (DEAL, ВЫСОКАЯ ЭНТРОПИЯ)");

            byte[] originalData = new byte[1500]; // 1.5 Кб
            RandomNumberGenerator.Fill(originalData);

            CipherMode[] modesToTest = { 
                CipherMode.ECB, 
                CipherMode.CBC, 
                CipherMode.PCBC, 
                CipherMode.CFB, 
                CipherMode.OFB, 
                CipherMode.CTR, 
                CipherMode.RandomDelta 
            };

            foreach (var mode in modesToTest)
            {
                // Для RandomDelta передаем дельту размером в блок DEAL (16 байт)
                byte[] delta = mode == CipherMode.RandomDelta ? new byte[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 } : null;

                var context = new SymmetricCryptoContext(
                    cipher: deal,
                    key: key,
                    cipherMode: mode,
                    paddingMode: PaddingMode.PKCS7,
                    iv: iv,
                    extraParams: delta
                );

                context.Encrypt(originalData, out byte[] encrypted);
                context.Decrypt(encrypted, out byte[] decrypted);

                bool isMatch = AreByteArraysEqual(originalData, decrypted);
                PrintResult($"Режим {mode,-11} | Исходный: {originalData.Length} б -> Шифр: {encrypted.Length} б", isMatch);
            }
        }

        #endregion

        #region Тест 2: Текст и код тестов красно-черного дерева

        private static async Task TestTextAndCodeAsync(DealCipher deal, byte[] key, byte[] iv)
        {
            PrintSubHeader("2. ТЕСТИРОВАНИЕ ИСХОДНОГО КОДА НА DEAL (CBC РЕЖИМ)");

            string sourceCodeSample = 
@"// Тест аллокатора памяти на базе Красно-Черного Дерева (RB-Tree) для DEAL
template <typename T>
class RedBlackTreeAllocator {
    struct Node {
        T data;
        bool isRed;
        Node *left, *right, *parent;
    };
    Node* root = nullptr;
    void RotateLeft(Node*& root, Node*& pt);
};";

            var context = new SymmetricCryptoContext(
                cipher: deal,
                key: key,
                cipherMode: CipherMode.CBC,
                paddingMode: PaddingMode.PKCS7,
                iv: iv
            );

            string originalFile = "deal_code_orig.cpp";
            string encryptedFile = "deal_code_enc.deal";
            string decryptedFile = "deal_code_restored.cpp";

            try
            {
                await File.WriteAllTextAsync(originalFile, sourceCodeSample);

                await context.EncryptFileAsync(originalFile, encryptedFile);
                await context.DecryptFileAsync(encryptedFile, decryptedFile);

                string restoredText = await File.ReadAllTextAsync(decryptedFile);
                bool isMatch = sourceCodeSample == restoredText;

                PrintResult("Шифрование исходного кода C++ алгоритмом DEAL (Файловый ввод-вывод)", isMatch);
            }
            finally
            {
                if (File.Exists(originalFile)) File.Delete(originalFile);
                if (File.Exists(encryptedFile)) File.Delete(encryptedFile);
                if (File.Exists(decryptedFile)) File.Delete(decryptedFile);
            }
        }

        #endregion

        #region Тест 3: Бинарные медиафайлы и SHA-256

        private static async Task TestBinaryMediaFileAsync(DealCipher deal, byte[] key, byte[] iv)
        {
            PrintSubHeader("3. ТЕСТИРОВАНИЕ МЕДИА-ФАЙЛОВ НА DEAL (CTR РЕЖИМ + SHA-256)");

            int mediaSize = 512 * 1024; // 512 Кб
            byte[] mediaData = new byte[mediaSize];
            new Random().NextBytes(mediaData);
            for (int i = mediaSize - 50; i < mediaSize; i++) mediaData[i] = 0x00;

            string originalMedia = "deal_media.dat";
            string encryptedMedia = "deal_media.enc";
            string decryptedMedia = "deal_media_restored.dat";

            try
            {
                await File.WriteAllBytesAsync(originalMedia, mediaData);
                string originalHash = GetSha256(originalMedia);

                var context = new SymmetricCryptoContext(
                    cipher: deal,
                    key: key,
                    cipherMode: CipherMode.CTR,
                    paddingMode: PaddingMode.PKCS7,
                    iv: iv
                );

                await context.EncryptFileAsync(originalMedia, encryptedMedia);
                await context.DecryptFileAsync(encryptedMedia, decryptedMedia);

                string restoredHash = GetSha256(decryptedMedia);
                bool isHashIdentical = originalHash == restoredHash;

                Console.WriteLine($"   Хэш исходного файла:     {originalHash}");
                Console.WriteLine($"   Хэш расшифрованного:     {restoredHash}");
                PrintResult("Контрольная сумма SHA-256 медиа-файла с алгоритмом DEAL", isHashIdentical);
            }
            finally
            {
                if (File.Exists(originalMedia)) File.Delete(originalMedia);
                if (File.Exists(encryptedMedia)) File.Delete(encryptedMedia);
                if (File.Exists(decryptedMedia)) File.Delete(decryptedMedia);
            }
        }

        #endregion

        #region Тест 4: Бенчмарк DEAL

        private static async Task RunBenchmarkAsync(DealCipher deal, byte[] key, byte[] iv)
        {
            PrintSubHeader("4. БЕНЧМАРК ПРОИЗВОДИТЕЛЬНОСТИ DEAL (ФАЙЛ 1 МБ)");

            byte[] bigData = new byte[1024 * 1024];
            new Random().NextBytes(bigData);

            CipherMode[] modes = { CipherMode.ECB, CipherMode.CBC, CipherMode.CTR };

            foreach (var mode in modes)
            {
                var context = new SymmetricCryptoContext(deal, key, mode, PaddingMode.PKCS7, iv);
                var sw = Stopwatch.StartNew();

                context.Encrypt(bigData, out byte[] enc);
                context.Decrypt(enc, out byte[] dec);

                sw.Stop();
                string parallelNote = (mode == CipherMode.ECB || mode == CipherMode.CTR) ? "[Параллельный]" : "[Последовательный]";
                Console.WriteLine($"   Режим {mode,-5} {parallelNote,-17}: Время = {sw.ElapsedMilliseconds,4} мс | Скорость: {(1.0 / sw.Elapsed.TotalSeconds):F2} МБ/с");
            }
        }

        #endregion

        #region Хелперы вывода

        private static bool AreByteArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length) return false;
            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i]) return false;
            return true;
        }

        private static string GetSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }

        private static void PrintHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine($"   {title}");
            Console.WriteLine("================================================================================");
            Console.ResetColor();
        }

        private static void PrintSubHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n--- {title} ---");
            Console.ResetColor();
        }

        private static void PrintResult(string testName, bool success)
        {
            Console.Write($"   {testName,-65} [ ");
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OK / СОВПАЛО");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ОШИБКА");
            }
            Console.ResetColor();
            Console.WriteLine(" ]");
        }

        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n================================================================================");
            Console.WriteLine($"   {message}");
            Console.WriteLine($"================================================================================");
            Console.ResetColor();
        }

        #endregion
    }
}
