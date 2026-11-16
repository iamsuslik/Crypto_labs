using System;
using System.Numerics;
using MyCryptoProject.lr2.task1;

namespace MyCryptoProject.lr2.task2
{
    public interface IPrimalityTest
    {
        bool IsPrime(BigInteger n, double minProbability);
    }
}
