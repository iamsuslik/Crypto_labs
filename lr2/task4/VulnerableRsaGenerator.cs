using System;
using System.Numerics;
using MyCryptoProject.lr2.task1;
using MyCryptoProject.lr2.task2;

namespace MyCryptoProject.lr2.task4
{
    public class VulnerableRsaGenerator
    {
        public static (BigInteger e, BigInteger n, BigInteger d, BigInteger phi) GenerateVulnerablePair(int bitLength = 256)
        {
            MillerRabinTest mr = new MillerRabinTest();
            Random rng = new Random();

            while (true)
            {
                BigInteger p = GenerateSmallPrime(bitLength / 2, mr);
                BigInteger q = GenerateSmallPrime(bitLength / 2, mr);
                BigInteger n = p * q;
                BigInteger phi = (p - 1) * (q - 1);

                for (BigInteger testD = 3; testD < 50000; testD += 2)
                {
                    if (CryptoMathService.GcdEuclid(testD, phi) == 1)
                    {
                        var ext = CryptoMathService.ExtendedGcd(testD, phi);
                        BigInteger testE = ext.x;
                        if (testE < 0) testE += phi;

                        if (BigInteger.Pow(3 * testD, 4) < n)
                        {
                            return (testE, n, testD, phi);
                        }
                    }
                }
            }
        }

        private static BigInteger GenerateSmallPrime(int bits, MillerRabinTest mr)
        {
            Random rng = new Random();
            int byteLength = Math.Max(bits / 8, 2);
            byte[] buf = new byte[byteLength];

            while (true)
            {
                rng.NextBytes(buf);
                buf[0] |= 1;
                buf[buf.Length - 1] |= 0x80;

                byte[] unsignedBuf = new byte[buf.Length + 1];
                Array.Copy(buf, unsignedBuf, buf.Length);

                BigInteger cand = new BigInteger(unsignedBuf);
                if (mr.IsPrime(cand, 0.99))
                {
                    return cand;
                }
            }
        }
    }
}
