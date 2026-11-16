using System;
using System.Numerics;

using MyCryptoProject.lr2.task1;
using MyCryptoProject.lr2.task2;


namespace MyCryptoProject.lr2.task3
{
    public class RsaService
    {
        public enum PrimalityTestType
        {
            Fermat,
            SolovayStrassen,
            MillerRabin
        }

        public BigInteger PublicKey_E { get; private set; }
        public BigInteger PublicKey_N { get; private set; }
        public BigInteger PrivateKey_D { get; private set; }

        private readonly KeyGenerator _keyGenerator;

        public RsaService(PrimalityTestType testType, double minProbability, int bitLength)
        {
            _keyGenerator = new KeyGenerator(testType, minProbability, bitLength);
        }

        public void GenerateKeys()
        {
            var keys = _keyGenerator.GenerateKeyPair();
            PublicKey_E = keys.E;
            PublicKey_N = keys.N;
            PrivateKey_D = keys.D;
        }

        public BigInteger Encrypt(BigInteger message)
        {
            if (message < 0 || message >= PublicKey_N)
                throw new ArgumentException("Сообщение должно быть в диапазоне [0, N-1].");

            return CryptoMathService.ManualModPow(message, PublicKey_E, PublicKey_N);
        }

        public BigInteger Decrypt(BigInteger cipherText)
        {
            return CryptoMathService.ManualModPow(cipherText, PrivateKey_D, PublicKey_N);
        }


        private class KeyGenerator
        {
            private readonly IPrimalityTest _primalityTest;
            private readonly double _minProbability;
            private readonly int _bitLength;

            public KeyGenerator(PrimalityTestType testType, double minProbability, int bitLength)
            {
                _minProbability = minProbability;
                _bitLength = bitLength;

                _primalityTest = testType switch
                {
                    PrimalityTestType.Fermat => new FermatTest(),
                    PrimalityTestType.SolovayStrassen => new SolovayStrassenTest(),
                    PrimalityTestType.MillerRabin => new MillerRabinTest(),
                    _ => throw new ArgumentException("Неизвестный тест простоты.")
                };
            }

            public (BigInteger E, BigInteger N, BigInteger D) GenerateKeyPair()
            {
                BigInteger p, q, n, phi, e, d;

                while (true)
                {
                    p = GeneratePrime();
                    q = GeneratePrime();

                    BigInteger diff = BigInteger.Abs(p - q);
                    if (diff < BigInteger.Pow(2, _bitLength / 2))
                    {
                        continue;
                    }

                    n = p * q;
                    phi = (p - 1) * (q - 1);

                    e = 65537;
                    if (CryptoMathService.GcdEuclid(e, phi) != 1)
                    {
                        continue;
                    }

                    var extendedGcd = CryptoMathService.ExtendedGcd(e, phi);
                    d = extendedGcd.x;
                    if (d < 0) d += phi;

                    BigInteger threeD = 3 * d;
                    BigInteger dCheck = BigInteger.Pow(threeD, 4);

                    if (dCheck <= n)
                    {
                        continue;
                    }

                    return (e, n, d);
                }
            }

            private BigInteger GeneratePrime()
            {
                Random rng = new Random();
                int byteLength = _bitLength / 8;
                byte[] bytes = new byte[byteLength];

                while (true)
                {
                    rng.NextBytes(bytes);

                    bytes[0] |= 1;
                    bytes[bytes.Length - 1] |= 0x80;

                    byte[] unsignedBytes = new byte[bytes.Length + 1];
                    Array.Copy(bytes, unsignedBytes, bytes.Length);

                    BigInteger candidate = new BigInteger(unsignedBytes);

                    if (_primalityTest.IsPrime(candidate, _minProbability))
                    {
                        return candidate;
                    }
                }
            }
        }
    }
}
