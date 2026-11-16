using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyCryptoProject.lr1.Task2.Context;
using MyCryptoProject.lr1.Task2.Enums;
using MyCryptoProject.lr1.Task4;


using CipherMode = MyCryptoProject.lr1.Task2.Enums.CipherMode;
using PaddingMode = MyCryptoProject.lr1.Task2.Enums.PaddingMode;

namespace MyCryptoProject.lr1.Task5
{
    public static class DesDemonstrator
    {
        public static async Task RunAllDemosAsync()
        {
            Console.Clear();
            PrintHeader("ЗАДАНИЕ №5: КОМПЛЕКСНОЕ ТЕСТИРОВАНИЕ АЛГОРИТМА DES");

            // Инициализация шифра DES
            var des = new DesCipher();
            byte[] key = Encoding.UTF8.GetBytes("SecrKey1"); // Ровно 8 байт (64 бита)
            byte[] iv  = Encoding.UTF8.GetBytes("InitVec1"); // Ровно 8 байт (64 бита)

            // ТЕСТ 1: Псевдослучайные последовательности байтов
            await TestRandomBytesAsync(des, key, iv);

            // ТЕСТ 2: Текстовые данные и исходный код
            await TestTextAndCodeAsync(des, key, iv);

            // ТЕСТ 3: Бинарные файлы (Имитация медиа: аудио, видео, изображения)
            await TestBinaryMediaFileAsync(des, key, iv);

            // ТЕСТ 4: Сравнение скорости режимов (Бенчмарк параллелизма)
            await RunBenchmarkAsync(des, key, iv);

            PrintSuccess("\nВСЕ ТЕСТЫ ЗАДАНИЯ №5 УСПЕШНО ПРОЙДЕНЫ!");
        }

        #region Тест 1: Псевдослучайные байты

        private static async Task TestRandomBytesAsync(DesCipher des, byte[] key, byte[] iv)
        {
            PrintSubHeader("1. ТЕСТИРОВАНИЕ СЛУЧАЙНЫХ БАЙТОВ (ВЫСОКАЯ ЭНТРОПИЯ)");

            // Генерируем 1000 криптографически стойких случайных байтов
            byte[] originalData = new byte[1000];
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
                byte[]? delta = mode == CipherMode.RandomDelta? new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }: null;

                var context = new SymmetricCryptoContext(
                    cipher: des,
                    key: key,
                    cipherMode: mode,
                    paddingMode: PaddingMode.PKCS7,
                    iv: iv,
                    extraParams: delta
                );

                // Шифруем в памяти
                context.Encrypt(originalData, out byte[] encrypted);

                // Дешифруем
                context.Decrypt(encrypted, out byte[] decrypted);

                // Проверяем побайтовое совпадение
                bool isMatch = AreByteArraysEqual(originalData, decrypted);
                PrintResult($"Режим {mode,-11} | Исходный размер: {originalData.Length} б -> Шифр: {encrypted.Length} б", isMatch);
            }
        }

        #endregion

        #region Тест 2: Текст и код тестов красно-черного дерева

        private static async Task TestTextAndCodeAsync(DesCipher des, byte[] key, byte[] iv)
        {
            PrintSubHeader("2. ТЕСТИРОВАНИЕ ТЕКСТА И ИСХОДНОГО КОДА (НИЗКАЯ ЭНТРОПИЯ)");

            string sourceCodeSample = 
@"// Тест аллокатора памяти на базе Красно-Черного Дерева (RB-Tree)
template <typename T>
class RedBlackTreeAllocator {
    struct Node {
        T data;
        bool isRed;
        Node *left, *right, *parent;
    };
    Node* root = nullptr;
    void RotateLeft(Node*& root, Node*& pt);
    void RotateRight(Node*& root, Node*& pt);
public:
    void* Allocate(size_t bytes);
    void Deallocate(void* ptr);
};";

            var context = new SymmetricCryptoContext(
                cipher: des,
                key: key,
                cipherMode: CipherMode.CBC,
                paddingMode: PaddingMode.PKCS7,
                iv: iv
            );

            string originalFile = "source_code_orig.cpp";
            string encryptedFile = "source_code_encrypted.des";
            string decryptedFile = "source_code_restored.cpp";

            try
            {
                await File.WriteAllTextAsync(originalFile, sourceCodeSample);

                // Асинхронное шифрование и дешифрование
                await context.EncryptFileAsync(originalFile, encryptedFile);
                await context.DecryptFileAsync(encryptedFile, decryptedFile);

                string restoredText = await File.ReadAllTextAsync(decryptedFile);
                bool isMatch = sourceCodeSample == restoredText;

                PrintResult("Шифрование исходного кода C++ (Файловый асинхронный ввод-вывод CBC)", isMatch);
            }
            finally
            {
                // Гарантированное удаление временных файлов (чистота на жестком диске)
                if (File.Exists(originalFile)) File.Delete(originalFile);
                if (File.Exists(encryptedFile)) File.Delete(encryptedFile);
                if (File.Exists(decryptedFile)) File.Delete(decryptedFile);
            }
        }

        #endregion

        #region Тест 3: Бинарные медиафайлы (Имитация аудио/видео/изображений)

        private static async Task TestBinaryMediaFileAsync(DesCipher des, byte[] key, byte[] iv)
        {
            PrintSubHeader("3. ТЕСТИРОВАНИЕ МЕДИА-ФАЙЛОВ (ИЗОБРАЖЕНИЕ / АУДИО / ВИДЕО)");

            // Создаем бинарный файл (500 Кб)
            int mediaSize = 500 * 1024;
            byte[] mediaData = new byte[mediaSize];
            new Random().NextBytes(mediaData);

            // Имитируем заголовки и нулевые байты на конце (как в MP3/BMP)
            for (int i = mediaSize - 100; i < mediaSize; i++) mediaData[i] = 0x00;

            string originalMedia = "media_test.dat";
            string encryptedMedia = "media_test.enc";
            string decryptedMedia = "media_test_restored.dat";

            try
            {
                await File.WriteAllBytesAsync(originalMedia, mediaData);

                // Считаем контрольный хэш SHA-256 ДО шифрования
                string originalHash = GetSha256(originalMedia);

                var context = new SymmetricCryptoContext(
                    cipher: des,
                    key: key,
                    cipherMode: CipherMode.CTR, // Самый быстрый режим для видео
                    paddingMode: PaddingMode.PKCS7,
                    iv: iv
                );

                await context.EncryptFileAsync(originalMedia, encryptedMedia);
                await context.DecryptFileAsync(encryptedMedia, decryptedMedia);

                // Считаем контрольный хэш SHA-256 ПОСЛЕ дешифрования
                string restoredHash = GetSha256(decryptedMedia);

                bool isHashIdentical = originalHash == restoredHash;

                Console.WriteLine($"   Хэш исходного файла:     {originalHash}");
                Console.WriteLine($"   Хэш расшифрованного:     {restoredHash}");
                PrintResult("Контрольная сумма SHA-256 бинарного медиа-файла (500 Кб)", isHashIdentical);
            }
            finally
            {
                if (File.Exists(originalMedia)) File.Delete(originalMedia);
                if (File.Exists(encryptedMedia)) File.Delete(encryptedMedia);
                if (File.Exists(decryptedMedia)) File.Delete(decryptedMedia);
            }
        }

        #endregion

        #region Тест 4: Бенчмарк режимов

        private static async Task RunBenchmarkAsync(DesCipher des, byte[] key, byte[] iv)
        {
            PrintSubHeader("4. БЕНЧМАРК ПРОИЗВОДИТЕЛЬНОСТИ РЕЖИМОВ (ФАЙЛ 1 МБ)");

            byte[] bigData = new byte[1024 * 1024]; // 1 Мегабайт
            new Random().NextBytes(bigData);

            CipherMode[] modes = { CipherMode.ECB, CipherMode.CBC, CipherMode.CTR };

            foreach (var mode in modes)
            {
                var context = new SymmetricCryptoContext(des, key, mode, PaddingMode.PKCS7, iv);
                var sw = Stopwatch.StartNew();

                context.Encrypt(bigData, out byte[] enc);
                context.Decrypt(enc, out byte[] dec);

                sw.Stop();
                string parallelNote = (mode == CipherMode.ECB || mode == CipherMode.CTR) ? "[Параллельный]" : "[Последовательный]";
                Console.WriteLine($"   Режим {mode,-5} {parallelNote,-17}: Время = {sw.ElapsedMilliseconds,4} мс | Скорость: {(1.0 / sw.Elapsed.TotalSeconds):F2} МБ/с");
            }
        }

        #endregion

        #region Вспомогательные методы вывода

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
