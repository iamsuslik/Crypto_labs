using System;
using System.Numerics;
using MyCryptoProject.lr2.task1;

namespace MyCryptoProject.lr2.task2
{
    public class SolovayStrassenTest : BasePrimalityTest
    {
        protected override bool ExecuteIteration(BigInteger n, BigInteger a)
        {
            BigInteger exponent = (n - 1) / 2;
            BigInteger leftSide = CryptoMathService.ManualModPow(a, exponent, n);

            int jacobi = CryptoMathService.JacobiSymbol(a, n);

            BigInteger rightSide = jacobi;
            if (rightSide < 0) rightSide += n;

            return leftSide == rightSide;
        }
    }
}
