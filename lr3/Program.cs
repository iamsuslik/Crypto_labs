using System;
using System.IO;
using System.Text;
using MyCryptoProject.lr3.Task1;
using MyCryptoProject.lr3.Task2;
using MyCryptoProject.lr1.Task2.Context;
using MyCryptoProject.lr1.Task2.Enums;

namespace MyCryptoProject.lr3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== ЗАПУСК ДЕМОНСТРАЦИИ ЗАДАНИЯ №3 (Rijndael) ===\n");

            var irreduciblePolynomials = GaloisFieldService.GetAllIrreduciblePolynomials();
            
            byte[] testPolynomials = new byte[] { 0x1B, 0x1D, 0x2B }; 

            int[] keySizes = new int[] { 16, 24, 32 };

            foreach (var poly in testPolynomials)
            {
                Console.WriteLine($"\n---------------------------------------------------");
                Console.WriteLine($"[ТЕСТ] Используем неприводимый полином: 0x{poly:X2}");
                Console.WriteLine($"---------------------------------------------------");

                var rijndaelCipher = new Rijndael(poly);

                foreach (var keySize in keySizes)
                {
                    byte[] key = new byte[keySize];
                    new Random().NextBytes(key);

                    byte[] iv = new byte[16];
                    new Random().NextBytes(iv);

                    Console.WriteLine($"\n > Длина ключа: {keySize * 8} бит ({keySize} байт)");

                    foreach (var mode in new[] { CipherMode.ECB, CipherMode.CBC })
                    {
                        Console.WriteLine($"   * Проверяем режим шифрования: {mode}");

                        var cryptoContext = new SymmetricCryptoContext(
                            rijndaelCipher, 
                            key, 
                            mode, 
                            PaddingMode.PKCS7,
                            iv
                        );

                        // --- ТЕСТ 1: Псевдослучайная последовательность байтов ---
                        byte[] originalBytes = Encoding.UTF8.GetBytes("Привет, это секретное сообщение для тестирования Rijndael с полиномом 0x" + poly.ToString("X2") + "!");
                        
                        // Шифруем
                        cryptoContext.Encrypt(originalBytes, out byte[] encryptedBytes);
                        
                        // Расшифровываем
                        cryptoContext.Decrypt(encryptedBytes, out byte[] decryptedBytes);

                        string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
                        bool bytesMatch = originalBytes.AsSpan().SequenceEqual(decryptedBytes);
                        
                        Console.WriteLine($"     [Байты] Результат совпал с оригиналом: {bytesMatch}");

                        // --- ТЕСТ 2: Реальный файл  ---
                        // Создадим временный файл для теста
                        string testFileName = "test_file.txt";
                        string encFileName = $"test_file_{mode}_{keySize * 8}.enc";
                        string decFileName = $"test_file_{mode}_{keySize * 8}_dec.txt";

                        File.WriteAllText(testFileName, "Это содержимое реального файла, который мы зашифруем и расшифруем!");

                        // Асинхронное шифрование и расшифрование файла через контекст
                        cryptoContext.EncryptFileAsync(testFileName, encFileName).Wait();
                        cryptoContext.DecryptFileAsync(encFileName, decFileName).Wait();

                        // Проверяем, совпадает ли файл после расшифрования
                        byte[] originalFile = File.ReadAllBytes(testFileName);
                        byte[] decryptedFile = File.ReadAllBytes(decFileName);
                        bool fileMatch = originalFile.AsSpan().SequenceEqual(decryptedFile);

                        Console.WriteLine($"     [Файл .txt] Файл успешно зашифрован и расшифрован без потерь: {fileMatch}");

                        // Очищаем временные файлы
                        if (File.Exists(testFileName)) File.Delete(testFileName);
                        if (File.Exists(encFileName)) File.Delete(encFileName);
                        if (File.Exists(decFileName)) File.Delete(decFileName);
                    }
                }
            }

            Console.WriteLine("\n=== ВСЕ ДЕМОНСТРАЦИОННЫЕ ТЕСТЫ ЗАВЕРШЕНЫ! ===");
        }
    }
}
