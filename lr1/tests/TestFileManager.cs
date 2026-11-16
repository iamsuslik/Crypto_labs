using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MyCryptoProject.lr1.Task2.Context;
using MyCryptoProject.lr1.Task2.Enums;
using MyCryptoProject.lr1.Task2.Interfaces;

using CipherMode = MyCryptoProject.lr1.Task2.Enums.CipherMode;
using PaddingMode = MyCryptoProject.lr1.Task2.Enums.PaddingMode;

namespace MyCryptoProject.lr1.Tests
{
    public static class TestFileManager
    {
        public static async Task RunPhysicalFileTestsAsync(ISymmetricCipher cipher, byte[] key, byte[] iv, string cipherName)
        {
            Console.Clear();
            Console.WriteLine("================================================================================");
            Console.WriteLine($"   ПРОВЕРКА ФИЗИЧЕСКИХ ФАЙЛОВ ДЛЯ АЛГОРИТМА: {cipherName}");
            Console.WriteLine("================================================================================");

            // 1. Тест с файлом из одних нулей (проверка PKCS7 Padding)
            await TestPhysicalZerosFileAsync(cipher, key, iv);

            // 2. Тест с пустым файлом (0 байт)
            await TestPhysicalEmptyFileAsync(cipher, key, iv);

            // 3. Тест с файлом исходного кода 
            await TestPhysicalSourceCodeFileAsync(cipher, key, iv);

            await TestPresetFileAsync(cipher, key, iv, cipherName);

            RunRawMemoryTests(cipher, cipherName);

            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"   ТЕСТИРОВАНИЕ ДЛЯ {cipherName} ЗАВЕРШЕНО.");
            Console.WriteLine("================================================================================");
        }

        // --- ТЕСТ 1: ФАЙЛ ИЗ ОДНИХ НУЛЕЙ ---
        private static async Task TestPhysicalZerosFileAsync(ISymmetricCipher cipher, byte[] key, byte[] iv)
        {
            string fileName = "test_zeros_input.bin";
            string encName = "test_zeros_input.enc";
            string restName = "test_zeros_restored.bin";

            Console.Write($"   1. Тест файла из одних нулей (0x00, 33 байта).......... ");

            try
            {
                int fileLength = 33;
                byte[] zerosData = new byte[fileLength];
                Array.Fill(zerosData, (byte)0x00);

                await File.WriteAllBytesAsync(fileName, zerosData);

                var context = new SymmetricCryptoContext(cipher, key, CipherMode.CBC, PaddingMode.PKCS7, iv);

                await context.EncryptFileAsync(fileName, encName);
                await context.DecryptFileAsync(encName, restName);

                byte[] restoredData = await File.ReadAllBytesAsync(restName);

                bool success = (restoredData.Length == fileLength);
                if (success)
                {
                    for (int i = 0; i < fileLength; i++)
                    {
                        if (restoredData[i] != 0x00)
                        {
                            success = false;
                            break;
                        }
                    }
                }

                PrintResult(success);
            }
            catch (Exception ex)
            {
                PrintResult(false, ex.Message);
            }
            finally
            {
                CleanFiles(fileName, encName, restName);
            }
        }

        // --- ТЕСТ 2: ПУСТОЙ ФАЙЛ (0 БАЙТ) ---
        private static async Task TestPhysicalEmptyFileAsync(ISymmetricCipher cipher, byte[] key, byte[] iv)
        {
            string fileName = "test_empty_input.bin";
            string encName = "test_empty_input.enc";
            string restName = "test_empty_restored.bin";

            Console.Write($"   2. Тест пустого физического файла (0 байт)............ ");

            try
            {
                await File.WriteAllBytesAsync(fileName, new byte[0]);

                var context = new SymmetricCryptoContext(cipher, key, CipherMode.ECB, PaddingMode.PKCS7, iv);

                await context.EncryptFileAsync(fileName, encName);
                await context.DecryptFileAsync(encName, restName);

                byte[] restoredData = await File.ReadAllBytesAsync(restName);

                bool success = (restoredData != null && restoredData.Length == 0);

                PrintResult(success);
            }
            catch (Exception ex)
            {
                PrintResult(false, ex.Message);
            }
            finally
            {
                CleanFiles(fileName, encName, restName);
            }
        }

        // --- ТЕСТ 3: ФАЙЛ ИСХОДНОГО КОДА  ---
        private static async Task TestPhysicalSourceCodeFileAsync(ISymmetricCipher cipher, byte[] key, byte[] iv)
        {
            string fileName = "test_code.cpp";
            string encName = "test_code.enc";
            string restName = "test_code_restored.cpp";

            Console.Write($"   3. Тест файла исходного кода C++ (RB-Tree)............. ");

            try
            {
                string sourceCode = 
@"// Тест аллокатора на красно-черном дереве для защиты лабы
#include <iostream>
template<typename T>
class RBTree {
    void* root = nullptr;
};
int main() { std::cout << ""Secure!""; return 0; }";

                await File.WriteAllTextAsync(fileName, sourceCode);
                string originalHash = GetSha256(fileName);

                var context = new SymmetricCryptoContext(cipher, key, CipherMode.CTR, PaddingMode.PKCS7, iv);

                await context.EncryptFileAsync(fileName, encName);
                await context.DecryptFileAsync(encName, restName);

                string restoredHash = GetSha256(restName);
                string restoredText = await File.ReadAllTextAsync(restName);

                bool success = (originalHash == restoredHash) && (sourceCode == restoredText);
                PrintResult(success);
            }
            catch (Exception ex)
            {
                PrintResult(false, ex.Message);
            }
            finally
            {
                CleanFiles(fileName, encName, restName);
            }
        }

        // --- ТЕСТ 4: СЫРЫЕ МАССИВЫ БАЙТОВ В ПАРАМЕТРАХ (BYTE[]) ---
        public static void RunRawMemoryTests(ISymmetricCipher cipher, string cipherName)
        {
            Console.WriteLine($"\n--- ТЕСТЫ РАБОТЫ В ПАРАМЕТРХ (BYTE[]) ДЛЯ {cipherName} ---");

            byte[] key8 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] key16 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            byte[] key = (cipher.BlockSizeBytes == 16) ? key16 : key8;
            byte[] iv = new byte[cipher.BlockSizeBytes];
            Array.Fill(iv, (byte)0xAA);

            var context = new SymmetricCryptoContext(cipher, key, CipherMode.CBC, PaddingMode.PKCS7, iv);

            // 1. Тест: Пустой массив в памяти (0 байт)
            try
            {
                byte[] emptyInput = new byte[0];
                context.Encrypt(emptyInput, out byte[] enc);
                context.Decrypt(enc, out byte[] dec);

                bool success = (dec != null && dec.Length == 0);
                Console.Write($"   Пустой массив байтов (0 байт) ....................... ");
                PrintResult(success);
            }
            catch (Exception ex)
            {
                Console.Write($"   Пустой массив байтов (0 байт) ....................... ");
                PrintResult(false, ex.Message);
            }

            // 2. Тест: Массив из одних нулей в памяти (например, 32 байта)
            try
            {
                int size = 32;
                byte[] zerosInput = new byte[size]; // Все байты 0x00
                
                context.Encrypt(zerosInput, out byte[] enc);
                context.Decrypt(enc, out byte[] dec);

                bool success = (dec.Length == size);
                for (int i = 0; i < size; i++)
                {
                    if (dec[i] != 0x00) success = false;
                }

                Console.Write($"   Массив из одних нулей в памяти ({size} байт) ......... ");
                PrintResult(success);
            }
            catch (Exception ex)
            {
                Console.Write($"   Массив из одних нулей в памяти (32 байта) ......... ");
                PrintResult(false, ex.Message);
            }
        }

        // --- ТЕСТ 4: РАБОТА С ЗАРАНЕЕ ПОДГОТОВЛЕННЫМ ФАЙЛОМ ---
        public static async Task TestPresetFileAsync(ISymmetricCipher cipher, byte[] key, byte[] iv, string cipherName)
        {
            string presetFileName = "secret_document.txt";
            string encFileName = "secret_document.enc";
            string restoredFileName = "secret_document_restored.txt";

            Console.Write($"   4. Тест заранее подготовленного файла ................ ");

            try
            {
                // Шаг 1. Создаем (или подготавливаем) файл с уникальным содержимым
                string secretText = 
@"КРИПТОГРАФИЧЕСКИЙ ОТЧЕТ №1
----------------------------------------
Алгоритм: " + cipherName + @"
Статус: Защищено с использованием Сети Фейстеля, режима CBC и PKCS7 Padding.
Дата: 2024 год.
Конфиденциальная информация!";

                await File.WriteAllTextAsync(presetFileName, secretText, Encoding.UTF8);

                // Шаг 2. Инициализируем контекст шифрования
                var context = new SymmetricCryptoContext(
                    cipher: cipher,
                    key: key,
                    cipherMode: CipherMode.CBC,
                    paddingMode: PaddingMode.PKCS7,
                    iv: iv
                );

                // Шаг 3. Шифруем заранее подготовленный файл
                await context.EncryptFileAsync(presetFileName, encFileName);

                // Шаг 4. Расшифровываем файл обратно
                await context.DecryptFileAsync(encFileName, restoredFileName);

                // Шаг 5. Читаем расшифрованный текст и сравниваем с оригиналом
                string restoredText = await File.ReadAllTextAsync(restoredFileName, Encoding.UTF8);

                bool success = (secretText == restoredText);

                PrintResult(success);

                if (success)
                {
                    Console.WriteLine($"      [INFO] Исходный файл '{presetFileName}' успешно зашифрован в '{encFileName}'");
                    Console.WriteLine($"      [INFO] И расшифрован обратно в '{restoredFileName}' без потери данных!");
                }
            }
            catch (Exception ex)
            {
                PrintResult(false, ex.Message);
            }
            finally
            {
                CleanFiles(presetFileName, encFileName, restoredFileName);
            }
        }

        private static string GetSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }

        private static void PrintResult(bool success, string errorMessage = "")
        {
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[ УСПЕШНО / СОВПАЛО ]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ ОШИБКА ] {(string.IsNullOrEmpty(errorMessage) ? "" : $"({errorMessage})")}");
            }
            Console.ResetColor();
        }

        private static void CleanFiles(params string[] files)
        {
            foreach (var f in files)
            {
                if (File.Exists(f)) File.Delete(f);
                if (File.Exists(f + ".enc")) File.Delete(f + ".enc");
            }
        }
    }
}
