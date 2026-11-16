using System;
using System.Numerics;
using MyCryptoProject.lr2.task1;

namespace MyCryptoProject.lr2.task2
{
    public class FermatTest : BasePrimalityTest
    {
        protected override bool ExecuteIteration(BigInteger n, BigInteger a)
        {
            BigInteger result = CryptoMathService.ManualModPow(a, n - 1, n);
            return result == 1;
        }
    }
}
