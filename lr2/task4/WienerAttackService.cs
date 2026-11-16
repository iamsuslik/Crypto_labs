using System;
using System.Collections.Generic;
using System.Numerics;
using MyCryptoProject.lr2.task1;

namespace MyCryptoProject.lr2.task4
{
    public class ConvergentFraction
    {
        public BigInteger Numerator { get; set; }
        public BigInteger Denominator { get; set; }

        public ConvergentFraction(BigInteger numerator, BigInteger denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public override string ToString() => $"{Numerator}/{Denominator}";
    }

    public class WienerAttackResult
    {
        public bool Success { get; set; }
        public BigInteger PrivateKeyD { get; set; }
        public BigInteger EulerPhi { get; set; }
        public List<ConvergentFraction> Convergents { get; set; } = new List<ConvergentFraction>();
    }

    public class WienerAttackService
    {
        public WienerAttackResult Attack(BigInteger e, BigInteger n)
        {
            var result = new WienerAttackResult();

            List<BigInteger> quotients = new List<BigInteger>();
            BigInteger tempE = e;
            BigInteger tempN = n;

            while (tempN != 0)
            {
                BigInteger q = tempE / tempN;
                quotients.Add(q);
                BigInteger r = tempE % tempN;
                tempE = tempN;
                tempN = r;
            }

            BigInteger p0 = quotients[0], q0 = 1;
            BigInteger p1 = 0, q1 = 0;

            if (quotients.Count > 1)
            {
                p1 = quotients[0] * quotients[1] + 1;
                q1 = quotients[1];
            }

            result.Convergents.Add(new ConvergentFraction(p0, q0));
            if (quotients.Count > 1)
            {
                result.Convergents.Add(new ConvergentFraction(p1, q1));
            }

            BigInteger p_prev2 = p0, q_prev2 = q0;
            BigInteger p_prev1 = p1, q_prev1 = q1;

            for (int i = 2; i < quotients.Count; i++)
            {
                BigInteger q_current = quotients[i];
                BigInteger p_current = q_current * p_prev1 + p_prev2;
                BigInteger q_current_denom = q_current * q_prev1 + q_prev2;

                result.Convergents.Add(new ConvergentFraction(p_current, q_current_denom));

                BigInteger k = p_current;
                BigInteger d = q_current_denom;

                if (k == 0) continue;

                if ((e * d - 1) % k == 0)
                {
                    BigInteger phi = (e * d - 1) / k;

                    BigInteger b = n - phi + 1;
                    BigInteger discriminant = b * b - 4 * n;

                    if (discriminant >= 0)
                    {
                        BigInteger sqrtDisc = Sqrt(discriminant);

                        if (sqrtDisc * sqrtDisc == discriminant)
                        {
                            if ((b + sqrtDisc) % 2 == 0)
                            {
                                result.Success = true;
                                result.PrivateKeyD = d;
                                result.EulerPhi = phi;
                                return result;
                            }
                        }
                    }
                }

                p_prev2 = p_prev1;
                q_prev2 = q_prev1;
                p_prev1 = p_current;
                q_prev1 = q_current_denom;
            }

            result.Success = false;
            return result;
        }

        private BigInteger Sqrt(BigInteger n)
        {
            if (n == 0) return 0;
            if (n > 0)
            {
                BigInteger xPrev = n / 2;
                if (xPrev == 0) xPrev = 1;
                BigInteger x = (xPrev + n / xPrev) / 2;
                while (x < xPrev)
                {
                    xPrev = x;
                    x = (xPrev + n / xPrev) / 2;
                }
                BigInteger xNext = (x + n / x) / 2;
                while (xNext < x)
                {
                    x = xNext;
                    xNext = (x + n / x) / 2;
                }
                return x;
            }
            throw new ArgumentException("Корень из отрицательного числа.");
        }
    }
}
