using System;
using System.Numerics;

namespace MyCryptoProject.lr2.task1
{
    public static class CryptoMathService
    {
        public static BigInteger ManualModPow(BigInteger baseNum, BigInteger exponent, BigInteger modulus)
        {
            if (modulus == 1) return 0;
            BigInteger result = 1;

            baseNum = baseNum % modulus;
            if (baseNum < 0) baseNum += modulus;
            while (exponent > 0)
            {
                if (exponent % 2 != 0)
                {
                    result = (result * baseNum) % modulus;
                }

                baseNum = (baseNum * baseNum) % modulus;
                exponent /= 2;
            }

            return result;
        }

        public static int LegendreSymbol(BigInteger a, BigInteger p)
        {
            a = a % p;
            if (a < 0) a += p;
            if (a == 0) return 0;

            BigInteger exponent = (p - 1) / 2;
            BigInteger result = ManualModPow(a, exponent, p);
            if (result == 1) return 1;
            if (result == (p - 1)) return -1;
            return (int)result;
        }

        public static int JacobiSymbol(BigInteger a, BigInteger n)
        {
            if (n <= 0 || n % 2 == 0)
            {
                throw new ArgumentException("Знаменатель n должен быть нечетным положительным числом.");
            }

            a = a % n;
            if (a < 0) a += n;

            int t = 1;

            while (a != 0)
            {
                int s = 0;
                while (a % 2 == 0)
                {
                    a /= 2;
                    s++;
                }

                if (s % 2 != 0)
                {
                    BigInteger nMod8 = n % 8;
                    if (nMod8 == 3 || nMod8 == 5)
                    {
                        t = -t;
                    }
                }

                if (a % 4 == 3 && n % 4 == 3)
                {
                    t = -t;
                }

                BigInteger temp = n % a;
                n = a;
                a = temp;
            }

            if (n == 1)
            {
                return t;
            }
            else return 0;
        }

        public static BigInteger GcdEuclid(BigInteger a, BigInteger b)
        {
            a = BigInteger.Abs(a);
            b = BigInteger.Abs(b);
            while (b != 0)
            {
                BigInteger temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static (BigInteger gcd, BigInteger x, BigInteger y) ExtendedGcd(BigInteger a, BigInteger b)
        {
            BigInteger x0 = 1, y0 = 0;
            BigInteger x1 = 0, y1 = 1;

            while (b != 0)
            {
                BigInteger q = a / b;
                BigInteger r = a % b;

                a = b;
                b = r;

                BigInteger x2 = x0 - q * x1;
                BigInteger y2 = y0 - q * y1;

                x0 = x1;
                y0 = y1;
                x1 = x2;
                y1 = y2;
            }
            return (a, x0, y0);
        }

        public static BigInteger BinaryGcd(BigInteger a, BigInteger b)
        {
            a = BigInteger.Abs(a);
            b = BigInteger.Abs(b);

            if (a == 0) return b;
            if (b == 0) return a;

            int shift = 0;
            while ((a | b) % 2 == 0)
            {
                a >>= 1;
                b >>= 1;
                shift++;
            }

            while (a % 2 == 0) a >>= 1;

            do
            {
                while (b % 2 == 0) b >>= 1;

                if (a > b)
                {
                    BigInteger temp = a;
                    a = b;
                    b = temp;
                }
                b -= a;
            } while (b != 0);

            return a << shift;
        }

        public static BigInteger EulerPhiDefinition(BigInteger n)
        {
            BigInteger count = 0;
            for (BigInteger k = 1; k < n; k++)
            {
                if (GcdEuclid(k, n) == 1)
                {
                    count++;
                }
            }
            return count;
        }

        public static BigInteger EulerPhiFactorization(BigInteger n)
        {
            BigInteger result = n;
            BigInteger temp = n;

            for (BigInteger p = 2; p * p <= temp; p++)
            {
                if (temp % p == 0)
                {
                    while (temp % p == 0)
                    {
                        temp /= p;
                    }
                    result -= result / p;
                }
            }

            if (temp > 1)
            {
                result -= result / temp;
            }
            return result;
        }

        public static BigInteger EulerPhiFourier(long n) 
        {
            double sum = 0;
            for (long k = 1; k <= n; k++)
            {
                long gcd = (long)GcdEuclid(k, n);
                double angle = 2 * Math.PI * k / n;
                sum += gcd * Math.Cos(angle);
            }
            return (BigInteger)Math.Round(sum);
        }

        public static BigInteger CarmichaelLambda(BigInteger n)
        {
            if (n <= 0) throw new ArgumentException("Число должно быть положительным.");

            BigInteger Lcm(BigInteger a, BigInteger b) => (a * b) / GcdEuclid(a, b);

            BigInteger lambda = 1;
            BigInteger temp = n;

            for (BigInteger p = 2; p * p <= temp; p++)
            {
                if (temp % p == 0)
                {
                    BigInteger pPower = 1;
                    while (temp % p == 0)
                    {
                        pPower *= p;
                        temp /= p;
                    }

                    BigInteger currentLambda;
                    if (p == 2 && pPower >= 8)
                    {
                        currentLambda = (pPower / 4);
                    }
                    else if (p == 2 && pPower == 4)
                    {
                        currentLambda = 2;
                    }
                    else
                    {
                        currentLambda = (pPower / p) * (p - 1);
                    }

                    lambda = Lcm(lambda, currentLambda);
                }
            }

            if (temp > 1)
            {
                BigInteger currentLambda = temp - 1;
                lambda = Lcm(lambda, currentLambda);
            }

            return lambda;
        }
    }
}
