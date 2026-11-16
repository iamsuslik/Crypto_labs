using System;
using System.IO;
using System.Threading.Tasks;
using MyCryptoProject.lr1.Task2.Enums;
using MyCryptoProject.lr1.Task2.Interfaces;
using MyCryptoProject.lr1.Task2.Services;

namespace MyCryptoProject.lr1.Task2.Context
{
    public class SymmetricCryptoContext
    {
        private readonly ISymmetricCipher _cipher;
        private readonly CipherMode _cipherMode;
        private readonly PaddingMode _paddingMode;
        private readonly byte[] _iv;
        private readonly object[] _extraParams;

        public SymmetricCryptoContext(
            ISymmetricCipher cipher,
            byte[] key,
            CipherMode cipherMode,
            PaddingMode paddingMode,
            byte[] iv = null,
            params object[] extraParams)
        {
            _cipher = cipher ?? throw new ArgumentNullException(nameof(cipher));
            _cipherMode = cipherMode;
            _paddingMode = paddingMode;
            _iv = iv;
            _extraParams = extraParams;

            _cipher.SetKey(key);
        }

        #region Перегруженные методы шифрования

        public void Encrypt(byte[] input, out byte[] output)
        {
            output = EncryptProcess(input);
        }

        public async Task<byte[]> EncryptAsync(byte[] input)
        {
            return await Task.Run(() => EncryptProcess(input));
        }

        public async Task EncryptFileAsync(string inputFilePath, string outputFilePath)
        {
            int blockSize = _cipher.BlockSizeBytes;

            int bufferSize = Math.Max(65536, blockSize * 1024);
            if (bufferSize % blockSize != 0)
            {
                bufferSize = ((bufferSize / blockSize) + 1) * blockSize;
            }

            using (var inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
            using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
            {
                byte[] inputBytes = await File.ReadAllBytesAsync(inputFilePath);
                byte[] encryptedBytes = await Task.Run(() => EncryptProcess(inputBytes));
                await outputStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
            }
        }

        #endregion

        #region Перегруженные методы дешифрования

        public void Decrypt(byte[] input, out byte[] output)
        {
            output = DecryptProcess(input);
        }

        public async Task<byte[]> DecryptAsync(byte[] input)
        {
            return await Task.Run(() => DecryptProcess(input));
        }

        public async Task DecryptFileAsync(string inputFilePath, string outputFilePath)
        {
            using (var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true))
            {
                byte[] inputBytes = await File.ReadAllBytesAsync(inputFilePath);
                byte[] decryptedBytes = await Task.Run(() => DecryptProcess(inputBytes));
                await outputStream.WriteAsync(decryptedBytes, 0, decryptedBytes.Length);
            }
        }

        #endregion

        #region Ядро обработки

        private byte[] EncryptProcess(byte[] input)
        {
            int blockSize = _cipher.BlockSizeBytes;
            byte[] paddedInput = PaddingService.AddPadding(input, blockSize, _paddingMode);
            int totalBlocks = paddedInput.Length / blockSize;
            byte[] result = new byte[paddedInput.Length];

            byte[] initVector = _iv != null ? (byte[])_iv.Clone() : new byte[blockSize];

            switch (_cipherMode)
            {
                case CipherMode.ECB:
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] block = new byte[blockSize];
                        Buffer.BlockCopy(paddedInput, i * blockSize, block, 0, blockSize);
                        byte[] enc = _cipher.EncryptBlock(block);
                        Buffer.BlockCopy(enc, 0, result, i * blockSize, blockSize);
                    });
                    break;

                case CipherMode.CTR:
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] counterBlock = GenerateCtrCounter(initVector, i, blockSize);
                        byte[] keystream = _cipher.EncryptBlock(counterBlock);

                        for (int j = 0; j < blockSize; j++) {
                            result[i * blockSize + j] = (byte)(paddedInput[i * blockSize + j] ^ keystream[j]);
                        }
                    });
                    break;

                case CipherMode.CBC:
                    {
                        byte[] prev = (byte[])initVector.Clone();
                        for (int i = 0; i < totalBlocks; i++)
                        {
                            byte[] p = new byte[blockSize];
                            Buffer.BlockCopy(paddedInput, i * blockSize, p, 0, blockSize);

                            for (int j = 0; j < blockSize; j++)
                                p[j] ^= prev[j];

                            byte[] c = _cipher.EncryptBlock(p);
                            Buffer.BlockCopy(c, 0, result, i * blockSize, blockSize);
                            prev = c;
                        }
                    }
                    break;

                case CipherMode.PCBC:
                    {
                        byte[] prevP = new byte[blockSize];
                        byte[] prevC = (byte[])initVector.Clone();

                        for (int i = 0; i < totalBlocks; i++)
                        {
                            byte[] p = new byte[blockSize];
                            Buffer.BlockCopy(paddedInput, i * blockSize, p, 0, blockSize);

                            byte[] blockToEncrypt = new byte[blockSize];
                            for (int j = 0; j < blockSize; j++)
                                blockToEncrypt[j] = (byte)(p[j] ^ prevP[j] ^ prevC[j]);

                            byte[] c = _cipher.EncryptBlock(blockToEncrypt);
                            Buffer.BlockCopy(c, 0, result, i * blockSize, blockSize);

                            prevP = p;
                            prevC = c;
                        }
                    }
                    break;

                case CipherMode.CFB:
                    {
                        byte[] prevC = (byte[])initVector.Clone();
                        for (int i = 0; i < totalBlocks; i++)
                        {
                            byte[] enc = _cipher.EncryptBlock(prevC);
                            byte[] c = new byte[blockSize];

                            for (int j = 0; j < blockSize; j++)
                                c[j] = (byte)(paddedInput[i * blockSize + j] ^ enc[j]);

                            Buffer.BlockCopy(c, 0, result, i * blockSize, blockSize);
                            prevC = c;
                        }
                    }
                    break;

                case CipherMode.OFB:
                    {
                        byte[] o = (byte[])initVector.Clone();
                        for (int i = 0; i < totalBlocks; i++)
                        {
                            o = _cipher.EncryptBlock(o);
                            for (int j = 0; j < blockSize; j++)
                                result[i * blockSize + j] = (byte)(paddedInput[i * blockSize + j] ^ o[j]);
                        }
                    }
                    break;

                case CipherMode.RandomDelta:
                {
                    byte[] delta = GetDelta(blockSize);
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] counterBlock = GenerateDeltaCounter(initVector, delta, i, blockSize);
                        byte[] keystream = _cipher.EncryptBlock(counterBlock);

                        for (int j = 0; j < blockSize; j++)
                            result[i * blockSize + j] = (byte)(paddedInput[i * blockSize + j] ^ keystream[j]);
                    });
                }
                break;

                default:
                    throw new NotSupportedException($"Режим {_cipherMode} не поддерживается.");
            }

            return result;
        }

        private byte[] DecryptProcess(byte[] input)
        {
            int blockSize = _cipher.BlockSizeBytes;
            if (input.Length % blockSize != 0)
                throw new ArgumentException("Длина зашифрованных данных должна быть кратна размеру блока.");

            int totalBlocks = input.Length / blockSize;
            byte[] decryptedPadded = new byte[input.Length];
            byte[] initVector = _iv != null ? (byte[])_iv.Clone() : new byte[blockSize];

            switch (_cipherMode)
            {
                case CipherMode.ECB:
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] block = new byte[blockSize];
                        Buffer.BlockCopy(input, i * blockSize, block, 0, blockSize);
                        byte[] dec = _cipher.DecryptBlock(block);
                        Buffer.BlockCopy(dec, 0, decryptedPadded, i * blockSize, blockSize);
                    });
                    break;

                case CipherMode.CTR:
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] counterBlock = GenerateCtrCounter(initVector, i, blockSize);
                        byte[] keystream = _cipher.EncryptBlock(counterBlock);

                        for (int j = 0; j < blockSize; j++)
                            decryptedPadded[i * blockSize + j] = (byte)(input[i * blockSize + j] ^ keystream[j]);
                    });
                    break;

                case CipherMode.CBC:
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] c = new byte[blockSize];
                        Buffer.BlockCopy(input, i * blockSize, c, 0, blockSize);

                        byte[] dec = _cipher.DecryptBlock(c);

                        byte[] prevC = new byte[blockSize];
                        if (i == 0)
                            Buffer.BlockCopy(initVector, 0, prevC, 0, blockSize);
                        else
                            Buffer.BlockCopy(input, (i - 1) * blockSize, prevC, 0, blockSize);

                        for (int j = 0; j < blockSize; j++)
                            dec[j] ^= prevC[j];

                        Buffer.BlockCopy(dec, 0, decryptedPadded, i * blockSize, blockSize);
                    });
                    break;

                case CipherMode.CFB:
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] prevC = new byte[blockSize];
                        if (i == 0)
                            Buffer.BlockCopy(initVector, 0, prevC, 0, blockSize);
                        else
                            Buffer.BlockCopy(input, (i - 1) * blockSize, prevC, 0, blockSize);

                        byte[] enc = _cipher.EncryptBlock(prevC);

                        for (int j = 0; j < blockSize; j++)
                            decryptedPadded[i * blockSize + j] = (byte)(input[i * blockSize + j] ^ enc[j]);
                    });
                    break;

                case CipherMode.PCBC:
                    {
                        byte[] prevP = new byte[blockSize];
                        byte[] prevC = (byte[])initVector.Clone();

                        for (int i = 0; i < totalBlocks; i++)
                        {
                            byte[] c = new byte[blockSize];
                            Buffer.BlockCopy(input, i * blockSize, c, 0, blockSize);

                            byte[] dec = _cipher.DecryptBlock(c);
                            byte[] p = new byte[blockSize];

                            for (int j = 0; j < blockSize; j++)
                                p[j] = (byte)(dec[j] ^ prevP[j] ^ prevC[j]);

                            Buffer.BlockCopy(p, 0, decryptedPadded, i * blockSize, blockSize);
                            prevP = p;
                            prevC = c;
                        }
                    }
                    break;

                case CipherMode.OFB:
                    {
                        byte[] o = (byte[])initVector.Clone();
                        for (int i = 0; i < totalBlocks; i++)
                        {
                            o = _cipher.EncryptBlock(o);
                            for (int j = 0; j < blockSize; j++)
                                decryptedPadded[i * blockSize + j] = (byte)(input[i * blockSize + j] ^ o[j]);
                        }
                    }
                    break;

                case CipherMode.RandomDelta:
                {
                    byte[] delta = GetDelta(blockSize);
                    Parallel.For(0, totalBlocks, i =>
                    {
                        byte[] counterBlock = GenerateDeltaCounter(initVector, delta, i, blockSize);
                        byte[] keystream = _cipher.EncryptBlock(counterBlock);

                        for (int j = 0; j < blockSize; j++)
                            decryptedPadded[i * blockSize + j] = (byte)(input[i * blockSize + j] ^ keystream[j]);
                    });
                    break;
                }

                default:
                    throw new NotSupportedException($"Режим {_cipherMode} не поддерживается.");
            }

            return PaddingService.RemovePadding(decryptedPadded, blockSize, _paddingMode);
        }

        #endregion

        #region Вспомогательные методы

        private static byte[] GenerateCtrCounter(byte[] iv, int blockIndex, int blockSize)
        {
            byte[] counter = new byte[blockSize];
            Buffer.BlockCopy(iv, 0, counter, 0, Math.Min(iv.Length, blockSize));

            long carry = blockIndex;
            for (int i = blockSize - 1; i >= 0 && carry > 0; i--)
            {
                long sum = counter[i] + (carry & 0xFF);
                counter[i] = (byte)sum;
                carry = (carry >> 8) + (sum >> 8);
            }
            return counter;
        }

        private byte[] GetDelta(int blockSize)
        {
            byte[] delta;

            if (_extraParams != null && _extraParams.Length > 0 && _extraParams[0] is byte[] d)
            {
                delta = (byte[])d.Clone();
            }
            else
            {
                delta = new byte[blockSize];
                for (int i = 0; i < blockSize; i++) delta[i] = 0x5A;
            }

            delta[delta.Length - 1] |= 1;

            return delta;
        }


        private static byte[] GenerateDeltaCounter(byte[] initVector, byte[] delta, int blockIndex, int blockSize)
        {
            byte[] counter = new byte[blockSize];
            Buffer.BlockCopy(initVector, 0, counter, 0, Math.Min(initVector.Length, blockSize));

            if (blockIndex == 0)
                return counter;

            ulong carry = 0;
            for (int k = blockSize - 1; k >= 0; k--)
            {
                ulong product = (ulong)delta[k] * (ulong)blockIndex;
                ulong sum = (ulong)counter[k] + (product & 0xFF) + carry;

                counter[k] = (byte)sum;

                carry = (product >> 8) + (sum >> 8);
            }

            return counter;
        }

        #endregion
    }
}
