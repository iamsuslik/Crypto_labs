using System;
using System.Numerics;
using MyCryptoProject.lr2.task1;

namespace MyCryptoProject.lr2.task2
{
    public class MillerRabinTest : BasePrimalityTest
    {
        protected override bool ExecuteIteration(BigInteger n, BigInteger a)
        {
            BigInteger d = n - 1;
            int s = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            BigInteger x = CryptoMathService.ManualModPow(a, d, n);

            if (x == 1 || x == n - 1)
                return true;

            for (int r = 1; r < s; r++)
            {
                x = CryptoMathService.ManualModPow(x, 2, n);
                if (x == n - 1)
                    return true;
                if (x == 1) 
                    return false;

            }

            return false;
        }
    }
}
