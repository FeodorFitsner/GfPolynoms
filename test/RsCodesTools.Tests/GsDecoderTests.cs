﻿namespace AppliedAlgebra.RsCodesTools.Tests
{
    using System;
    using System.Collections.Generic;
    using Decoding.ListDecoder;
    using Decoding.ListDecoder.GsDecoderDependencies.InterpolationPolynomialBuilder;
    using Decoding.ListDecoder.GsDecoderDependencies.InterpolationPolynomialFactorisator;
    using Encoding;
    using GfAlgorithms.CombinationsCountCalculator;
    using GfPolynoms;
    using GfPolynoms.Extensions;
    using GfPolynoms.GaloisFields;
    using JetBrains.Annotations;
    using Xunit;

    public class GsDecoderTests
    {
        private readonly GsDecoder _decoder;

        [UsedImplicitly]
        public static readonly IEnumerable<object[]> DecoderTestsData;

        private static Tuple<FieldElement, FieldElement>[] AddRandomNoise(Tuple<FieldElement, FieldElement>[] codeword, int errorsCount)
        {
            var random = new Random();
            var errorsPositions = new HashSet<int>();
            while (errorsPositions.Count < errorsCount)
                errorsPositions.Add(random.Next(codeword.Length));

            var one = codeword[0].Item1.Field.One();
            foreach (var errorPosition in errorsPositions)
                codeword[errorPosition].Item2.Add(one);

            return codeword;
        }

        private static Tuple<FieldElement, FieldElement>[] AddNoise(Tuple<FieldElement, FieldElement>[] codeword, params int[] errorsPositions)
        {
            var one = codeword[0].Item1.Field.One();
            foreach (var errorPosition in errorsPositions)
                codeword[errorPosition].Item2.Add(one);

            return codeword;
        }

        private static object[] PrepareTestsData(int n, int k, IEncoder encoder, Polynomial informationPolynomial, int randomErrorsCount)
        {
            return new object[]
                   {
                       n, k,
                       AddRandomNoise(encoder.Encode(n, informationPolynomial), randomErrorsCount), n - randomErrorsCount,
                       informationPolynomial
                   };
        }

        private static object[] PrepareTestsData(int n, int k, IEncoder encoder, Polynomial informationPolynomial, params int[] errorsPositions)
        {
            return new object[]
                   {
                       n, k,
                       AddNoise(encoder.Encode(n, informationPolynomial), errorsPositions), n - errorsPositions.Length,
                       informationPolynomial
                   };
        }

        static GsDecoderTests()
        {
            var gf8 = new PrimePowerOrderField(8, new Polynomial(new PrimeOrderField(2), 1, 1, 0, 1));
            var gf9 = new PrimePowerOrderField(9, new Polynomial(new PrimeOrderField(3), 1, 0, 1));
            var encoder = new Encoder();

            DecoderTestsData = new[]
                               {
                                   PrepareTestsData(7, 3, encoder, new Polynomial(gf8, 1, 2, 3), 2, 3, 6),
                                   PrepareTestsData(7, 3, encoder, new Polynomial(gf8, 1, 2, 3), 3),
                                   PrepareTestsData(7, 3, encoder, new Polynomial(gf8, 7, 4, 1), 3),
                                   PrepareTestsData(7, 3, encoder, new Polynomial(gf8, 0, 2), 3),
                                   PrepareTestsData(7, 3, encoder, new Polynomial(gf8, 0, 0, 3), 3),
                                   PrepareTestsData(8, 5, encoder, new Polynomial(gf9, 0, 0, 3, 1, 1), 2)
                               };
        }

        public GsDecoderTests()
        {
            _decoder = new GsDecoder(new KotterAlgorithmBasedBuilder(new PascalsTriangleBasedCalculator()), new RrFactorizator());
        }

        [Theory]
        [MemberData(nameof(DecoderTestsData))]
        public void ShouldFindOriginalInformationWordAmongPossibleVariants(int n, int k, Tuple<FieldElement, FieldElement>[] decodedCodeword, int minCorrectValuesCount, Polynomial expectedInformationPolynomial)
        {
            // When
            var possibleVariants = _decoder.Decode(n, k, decodedCodeword, minCorrectValuesCount);

            // Then
            Assert.Contains(expectedInformationPolynomial, possibleVariants);
        }
    }
}