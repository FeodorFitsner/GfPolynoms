﻿namespace AppliedAlgebra.RsCodesTools.Encoding
{
    using System;
    using GfPolynoms;
    using GfPolynoms.Extensions;

    /// <summary>
    /// Implementation of the encoder contract for the Reed-Solomon code encoder
    /// </summary>
    public class Encoder : IEncoder
    {
        /// <summary>
        /// Method for computing Reed-Solomon code's codeword for information polynomial <paramref name="informationPolynomial"/>
        /// </summary>
        /// <param name="n">Codeword length</param>
        /// <param name="informationPolynomial">Information polynomial</param>
        /// <returns>Computed codeword</returns>
        public Tuple<FieldElement, FieldElement>[] Encode(int n, Polynomial informationPolynomial)
        {
            var field = informationPolynomial.Field;
            var codeword = new Tuple<FieldElement, FieldElement>[n];

            for (var i = 0; i < n; i++)
            {
                var variableValue = field.GetGeneratingElementPower(i);
                codeword[i] = Tuple.Create(field.CreateElement(variableValue),
                    field.CreateElement(informationPolynomial.Evaluate(variableValue)));
            }

            return codeword;
        }
    }
}