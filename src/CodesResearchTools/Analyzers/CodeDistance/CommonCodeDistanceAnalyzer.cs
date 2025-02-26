﻿namespace AppliedAlgebra.CodesResearchTools.Analyzers.CodeDistance
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GfAlgorithms.Extensions;
    using GfPolynoms;
    using GfPolynoms.GaloisFields;
    using Microsoft.Extensions.Logging;

    public class CommonCodeDistanceAnalyzer :  CodeDistanceAnalyzerBase
    {
        protected override int AnalyzeInternal(
            GaloisField field, 
            int informationWordLength, 
            Func<int[], FieldElement[]> encodingProcedure,
            CodeDistanceAnalyzerOptions options)
        {
            var processedPairsCount = 0L;
            var codeDistance = int.MaxValue;
            Parallel.ForEach(
                GenerateMappings(field, encodingProcedure, new int[informationWordLength], 0),
                new ParallelOptions {MaxDegreeOfParallelism = options.MaxDegreeOfParallelism},
                () => int.MaxValue,
                (mapping, loopState, localCodeDistance) =>
                {
                    foreach (var (_, codeword) in GenerateMappings(field, encodingProcedure, mapping.informationWord, 0).Skip(1))
                    {
                        if (Interlocked.Increment(ref processedPairsCount) % options.LoggingResolution == 0)
                            Logger.LogInformation("Processed {processedPairsCount} pairs, code distance {codeDistance}", processedPairsCount, localCodeDistance);

                        localCodeDistance = Math.Min(localCodeDistance, mapping.codeword.ComputeHammingDistance(codeword));
                    }

                    return localCodeDistance;
                },
                localCodeDistance => { codeDistance = Math.Min(codeDistance, localCodeDistance); }
            );

            return codeDistance;
        }

        public CommonCodeDistanceAnalyzer(ILogger<CommonCodeDistanceAnalyzer> logger) : base(logger)
        {
        }
    }
}