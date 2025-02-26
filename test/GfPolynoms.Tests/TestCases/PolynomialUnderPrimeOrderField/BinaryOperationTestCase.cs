﻿namespace AppliedAlgebra.GfPolynoms.Tests.TestCases.PolynomialUnderPrimeOrderField
{
    using GaloisFields;

    public class BinaryOperationTestCase
    {
        public PrimeOrderField Field { get; set; }

        public int[] FirstOperandCoefficients { get; set; }

        public int[] SecondOperandCoefficients { get; set; }

        public int[] ExpectedResultCoefficients { get; set; }
    }
}