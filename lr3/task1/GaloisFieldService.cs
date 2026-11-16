using System;
using System.Collections.Generic;

namespace MyCryptoProject.lr3.Task1
{
    public static class GaloisFieldService
    {
        public static byte Add(byte a, byte b)
        {
            return (byte)(a ^ b);
        }

        public static byte Multiply(byte a, byte b, byte irreduciblePolynomial)
        {
            if (!IsIrreducible(irreduciblePolynomial))
            {
                throw new ArgumentException("Указанный модуль не является неприводимым полиномом 8-й степени!", nameof(irreduciblePolynomial));
            }

            byte result = 0;
            ushort tempA = a;
            byte tempB = b;

            for (int i = 0; i < 8; i++)
            {
                if ((tempB & 1) != 0)
                {
                    result = (byte)(result ^ tempA);
                }

                bool highBitSet = (tempA & 0x80) != 0;

                tempA = (ushort)(tempA << 1);

                if (highBitSet)
                {
                    tempA = (ushort)(tempA ^ irreduciblePolynomial);
                }

                tempB >>= 1;
            }

            return result;
        }

        public static byte Inverse(byte a, byte irreduciblePolynomial)
        {
            if (!IsIrreducible(irreduciblePolynomial))
            {
                throw new ArgumentException("Указанный модуль не является неприводимым полиномом!", nameof(irreduciblePolynomial));
            }

            if (a == 0)
            {
                throw new DivideByZeroException("У нулевого элемента нет обратного.");
            }

            for (int i = 1; i < 256; i++)
            {
                byte candidate = (byte)i;
                if (Multiply(a, candidate, irreduciblePolynomial) == 1)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("Обратный элемент не найден.");
        }
        public static bool IsIrreducible(byte polynomial)
        {
            var allIrreducibles = GetAllIrreduciblePolynomials();
            return allIrreducibles.Contains(polynomial);
        }

        public static HashSet<byte> GetAllIrreduciblePolynomials()
        {
            return new HashSet<byte>
            {
                0x1B, 0x1D, 0x2B, 0x2D, 0x39, 0x3F, 0x4D, 0x5F, 
                0x63, 0x65, 0x69, 0x71, 0x77, 0x7B, 0x87, 0x8B, 
                0x8D, 0x97, 0x9D, 0xA3, 0xA9, 0xB1, 0xBD, 0xC3, 
                0xCF, 0xD3, 0xDF, 0xE7, 0xEB, 0xF3
            };
        }


        public static List<int[]> FactorizePolynomial(int[] poly)
        {
            poly = Normalize(poly);

            if (GetDegree(poly) <= 0)
            {
                return new List<int[]>();
            }

            int[] deriv = Derivative(poly);
            int[] g = PolynomialGcd(poly, deriv);

            List<int[]> resultFactors = new List<int[]>();

            if (GetDegree(g) > 0)
            {
                int[] reduced = PolynomialDivision(poly, g).quotient;
                resultFactors.AddRange(FactorizePolynomial(reduced));
                resultFactors.AddRange(FactorizePolynomial(g));
                return resultFactors;
            }

            if (GetDegree(poly) == 1)
            {
                resultFactors.Add(poly);
                return resultFactors;
            }

            int deg = GetDegree(poly);
            Random rand = new Random();

            while (true)
            {
                int[] u = GenerateRandomPolynomial(deg, rand);

                int[] gcd = PolynomialGcd(poly, u);

                if (GetDegree(gcd) > 0 && GetDegree(gcd) < deg)
                {
                    int[] other = PolynomialDivision(poly, gcd).quotient;
                    resultFactors.AddRange(FactorizePolynomial(gcd));
                    resultFactors.AddRange(FactorizePolynomial(other));
                    return resultFactors;
                }
            }
        }

        #region Вспомогательная полиномиальная арифметика для произвольной степени

        // Удаление ведущих нулей 
        private static int[] Normalize(int[] poly)
        {
            int lastNonZero = -1;
            for (int i = poly.Length - 1; i >= 0; i--)
            {
                if (poly[i] != 0)
                {
                    lastNonZero = i;
                    break;
                }
            }

            if (lastNonZero == -1) return new int[] { 0 };

            int[] result = new int[lastNonZero + 1];
            Array.Copy(poly, result, lastNonZero + 1);
            return result;
        }

        private static int GetDegree(int[] poly)
        {
            poly = Normalize(poly);
            return poly.Length - 1;
        }

        // Сложение полиномов в GF(2)
        private static int[] PolynomialAdd(int[] a, int[] b)
        {
            int maxLen = Math.Max(a.Length, b.Length);
            int[] result = new int[maxLen];
            for (int i = 0; i < maxLen; i++)
            {
                int valA = i < a.Length ? a[i] : 0;
                int valB = i < b.Length ? b[i] : 0;
                result[i] = valA ^ valB;
            }
            return Normalize(result);
        }

        // Умножение полиномов в GF(2)
        private static int[] PolynomialMultiply(int[] a, int[] b)
        {
            int[] result = new int[a.Length + b.Length - 1];
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0) continue;
                for (int j = 0; j < b.Length; j++)
                {
                    if (b[j] == 0) continue;
                    result[i + j] ^= 1;
                }
            }
            return Normalize(result);
        }

        // Деление полиномов с остатком в GF(2) 
        private static (int[] quotient, int[] remainder) PolynomialDivision(int[] numerator, int[] denominator)
        {
            numerator = Normalize(numerator);
            denominator = Normalize(denominator);

            int degNum = GetDegree(numerator);
            int degDen = GetDegree(denominator);

            if (degNum < degDen)
            {
                return (new int[] { 0 }, numerator);
            }

            int[] quotient = new int[degNum - degDen + 1];
            int[] remainder = (int[])numerator.Clone();

            for (int i = degNum; i >= degDen; i--)
            {
                int remDeg = GetDegree(remainder);
                if (remDeg == i && remainder[i] != 0)
                {
                    int factorDegree = i - degDen;
                    quotient[factorDegree] = 1;

                    int[] shiftedDen = new int[factorDegree + denominator.Length];
                    Array.Copy(denominator, 0, shiftedDen, factorDegree, denominator.Length);
                    remainder = PolynomialAdd(remainder, shiftedDen);
                }
            }

            return (Normalize(quotient), Normalize(remainder));
        }

        private static int[] PolynomialGcd(int[] a, int[] b)
        {
            a = Normalize(a);
            b = Normalize(b);

            while (GetDegree(b) >= 0 && !(b.Length == 1 && b[0] == 0))
            {
                var div = PolynomialDivision(a, b);
                a = b;
                b = div.remainder;
            }

            return a;
        }

        // Формальная производная полинома в GF(2)
        private static int[] Derivative(int[] poly)
        {
            int deg = GetDegree(poly);
            if (deg <= 0) return new int[] { 0 };

            int[] deriv = new int[deg];
            for (int i = 1; i <= deg; i++)
            {
                if (i % 2 != 0)
                {
                    deriv[i - 1] = poly[i];
                }
            }
            return Normalize(deriv);
        }

        private static int[] GenerateRandomPolynomial(int deg, Random rand)
        {
            int[] poly = new int[deg];
            for (int i = 0; i < deg; i++)
            {
                poly[i] = rand.Next(2); // 0 или 1
            }
            return Normalize(poly);
        }

        #endregion
    }
}
