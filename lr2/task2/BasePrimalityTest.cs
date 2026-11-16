using System;
using System.Numerics;
using MyCryptoProject.lr2.task1;

namespace MyCryptoProject.lr2.task2
{
    public abstract class BasePrimalityTest : IPrimalityTest
    {
        public virtual bool IsPrime(BigInteger n, double minProbability)
        {
            if (n <= 1) return false;
            if (n == 2 || n == 3) return true;
            if (n % 2 == 0) return false;

            int k = CalculateIterationsCount(minProbability);

            for (int i = 0; i < k; i++)
            {
                BigInteger a = GenerateRandomBase(n);

                if (!ExecuteIteration(n, a))
                {
                    return false;
                }
            }

            return true; 
        }

        protected abstract bool ExecuteIteration(BigInteger n, BigInteger a);

        protected BigInteger GenerateRandomBase(BigInteger n)
        {
            Random rng = new Random();
            byte[] bytes = n.ToByteArray();
            BigInteger a;
            do
            {
                rng.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; 
                a = new BigInteger(bytes);
            } while (a < 2 || a >= n - 1);

            return a;
        }

        private int CalculateIterationsCount(double minProbability)
        {
            if (minProbability < 0.5 || minProbability >= 1.0)
                throw new ArgumentException("Вероятность должна быть в диапазоне [0.5, 1).");

            double errorRate = 1.0 - minProbability;
            int k = (int)Math.Ceiling(-Math.Log(errorRate) / Math.Log(4));
            return Math.Max(k, 5);
        }
    }
}
