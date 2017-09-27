using System;
using System.Collections.Generic;
using System.Linq;
using CurrenciesFetcher.Models;
using JustConveyor;
using JustConveyor.Contracts.Attributes;
using JustConveyor.Contracts.Dependencies.Attributes;
using JustConveyor.Contracts.Pipelining;
using JustConveyor.Contracts.Pipelining.Attributes;
using JustConveyor.Contracts.Pipelining.Contexts;
using JustConveyor.Contracts.Utils;

namespace CurrenciesFetcher
{
    [Injecting]
    [PipelineBuilder("currencies-by-dates-analyzer")]
    public class CurrenciesAnalizer
    {
        [Splitter("generate-dates")]
        public List<UnitContext> GenerateDiapason(DatesDiapason diapason, UnitContext ctx)
        {
            var diapasonSize = diapason.To - diapason.From;

            return Enumerable.Range(0, (int) diapasonSize.TotalDays + 1)
                .Select(dayNumber => diapason.From.AddDays(dayNumber))
                .Select(el => new UnitContext(ctx.ProcessingId, $"{el}", el, ctx.Headers)).ToList();
        }

        [Processor("analyze")]
        public double Analyze(IEnumerable<double?> currencies)
        {
            return currencies.Where(el => el.HasValue).Select(el => el.Value).Max();
        }

        [Blueprint]
        public PipelineDescriptor CurrenciesAnalyzerBlueprint()
        {
            var blueprint = PipelineBlueprint.CreateBlueprint<DatesDiapason>("AnalyzeCurrenciesRatesByDiapason")
                .Split<CurrenciesAnalizer>("generate-dates")
                .Apply<FixerIOCurrenciesFetcher>("date-rate")
                .CollectAndCast<double?>()
                .Apply<CurrenciesAnalizer>("analyze")
                .Apply<Finalizer>();

            return new PipelineDescriptor
            {
                Blueprint = blueprint,
                ConcurrentLinesNumber = 1,
                ForType = true
            };
        }
    }
}