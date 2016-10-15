﻿namespace AppliedAlgebra.RsCodesTools.ListDecoder.GsDecoderDependencies.InterpolationPolynomialBuilder
{
    using System;
    using System.Collections.Generic;
    using GfAlgorithms.BiVariablePolynomials;
    using GfAlgorithms.CombinationsCountCalculator;
    using GfAlgorithms.Extensions;
    using GfAlgorithms.LinearSystemSolver;
    using GfPolynoms;
    using GfPolynoms.Extensions;
    using GfPolynoms.GaloisFields;

    public class SimplePolynomialBuilder : IInterpolationPolynomialBuilder
    {
        private readonly ICombinationsCountCalculator _combinationsCountCalculator;
        private readonly ILinearSystemSolver _linearSystemSolver;

        private static int GetVariableIndexByMonomial(
            IDictionary<Tuple<int, int>, int> variableIndexByMonomial,
            IDictionary<int, Tuple<int, int>> monomialByVariableIndex,
            int xDegree, int yDegree)
        {
            var monomial = new Tuple<int, int>(xDegree, yDegree);
            int variableIndex;

            if (variableIndexByMonomial.TryGetValue(monomial, out variableIndex) == false)
            {
                variableIndex = variableIndexByMonomial.Count;
                variableIndexByMonomial[monomial] = variableIndex;
                monomialByVariableIndex[variableIndex] = monomial;
            }
            return variableIndex;
        }

        private List<List<Tuple<int, FieldElement>>> BuildEquationsSystem(
            IDictionary<Tuple<int, int>, int> variableIndexByMonomial,
            IDictionary<int, Tuple<int, int>> monomialByVariableIndex,
            GaloisField field, Tuple<int, int> degreeWeight, int maxWeightedDegree, int maxXDegree,
            IEnumerable<Tuple<FieldElement, FieldElement>> roots, int rootsMultiplicity)
        {
            var equations = new List<List<Tuple<int, FieldElement>>>();
            var combinationsCache = new FieldElement[Math.Max(maxWeightedDegree/degreeWeight.Item1, maxWeightedDegree/degreeWeight.Item2) + 1][]
                    .MakeSquare();

            foreach (var root in roots)
                for (var r = 0; r < rootsMultiplicity; r++)
                    for (var s = 0; r + s < rootsMultiplicity; s++)
                    {
                        var equation = new List<Tuple<int, FieldElement>>();
                        equations.Add(equation);

                        for (var i = r; i <= maxXDegree; i++)
                            for (var j = s; i*degreeWeight.Item1 + j*degreeWeight.Item2 <= maxWeightedDegree; j++)
                            {
                                var variableIndex = GetVariableIndexByMonomial(variableIndexByMonomial, monomialByVariableIndex, i, j);
                                var coefficient = _combinationsCountCalculator.Calculate(field, i, r, combinationsCache)
                                                  *_combinationsCountCalculator.Calculate(field, j, s, combinationsCache)
                                                  *FieldElement.Pow(root.Item1, i - r)
                                                  *FieldElement.Pow(root.Item2, j - s);

                                equation.Add(new Tuple<int, FieldElement>(variableIndex, coefficient));
                            }
                    }

            return equations;
        }

        private SystemSolution SolveEquationsSystem(GaloisField field,
            int variablesCount, IReadOnlyList<List<Tuple<int, FieldElement>>> equationsSystem)
        {
            var linearSystemMatrix = new FieldElement[equationsSystem.Count, variablesCount];
            var zeros = new FieldElement[equationsSystem.Count];
            for (var i = 0; i < equationsSystem.Count; i++)
            {
                zeros[i] = field.Zero();
                for (var j = 0; j < variablesCount; j++)
                    linearSystemMatrix[i, j] = field.Zero();
            }
            for (var i = 0; i < equationsSystem.Count; i++)
                for (var j = 0; j < equationsSystem[i].Count; j++)
                    linearSystemMatrix[i, equationsSystem[i][j].Item1] = equationsSystem[i][j].Item2;

            return _linearSystemSolver.Solve(linearSystemMatrix, zeros);
        }

        private static BiVariablePolynomial ConstructInterpolationPolynomial(BiVariablePolynomial blankPolynomial,
            SystemSolution systemSolution, IReadOnlyDictionary<int, Tuple<int, int>> monomialByVariableIndex)
        {
            for (var i = 0; i < systemSolution.Solution.Length; i++)
                blankPolynomial[monomialByVariableIndex[i]] = systemSolution.Solution[i];

            if (blankPolynomial.IsZero)
                throw new NonTrivialPolynomialNotFoundException();
            return blankPolynomial;
        }

        public BiVariablePolynomial Build(Tuple<int, int> degreeWeight, int maxWeightedDegree, 
            Tuple<FieldElement, FieldElement>[] roots, int rootsMultiplicity)
        {
            if (degreeWeight == null)
                throw new ArgumentNullException(nameof(degreeWeight));
            if (roots == null)
                throw new ArgumentNullException(nameof(roots));
            if (roots.Length == 0)
                throw new ArgumentException($"{nameof(roots)} is empty");

            var field = roots[0].Item1.Field;
            var resultPolynomial = new BiVariablePolynomial(field);

            var variableIndexByMonomial = new Dictionary<Tuple<int, int>, int>();
            var monomialByVariableIndex = new Dictionary<int, Tuple<int, int>>();

            var equationsSystem = BuildEquationsSystem(variableIndexByMonomial, monomialByVariableIndex,
                field, degreeWeight, maxWeightedDegree, maxWeightedDegree/degreeWeight.Item1,
                roots, rootsMultiplicity);

            var systemSolution = SolveEquationsSystem(field, variableIndexByMonomial.Count, equationsSystem);

            return ConstructInterpolationPolynomial(resultPolynomial, systemSolution, monomialByVariableIndex);
        }

        public SimplePolynomialBuilder(ICombinationsCountCalculator combinationsCountCalculator, ILinearSystemSolver linearSystemSolver)
        {
            if (combinationsCountCalculator == null)
                throw new ArgumentNullException(nameof(combinationsCountCalculator));
            if (linearSystemSolver == null)
                throw new ArgumentNullException(nameof(linearSystemSolver));

            _combinationsCountCalculator = combinationsCountCalculator;
            _linearSystemSolver = linearSystemSolver;
        }
    }
}