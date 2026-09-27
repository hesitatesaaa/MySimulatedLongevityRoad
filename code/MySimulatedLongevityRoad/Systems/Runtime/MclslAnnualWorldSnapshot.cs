using System.Collections.Generic;
using System.Diagnostics;
using MySimulatedLongevityRoad.Core;
using MySimulatedLongevityRoad.Data;

namespace MySimulatedLongevityRoad.Systems;

internal sealed class MclslAnnualWorldSnapshot
{
    private static readonly Actor[] EmptyActors = System.Array.Empty<Actor>();
    private static readonly List<Actor> ReusableLineageActors = new(1024);

    internal IReadOnlyList<Actor> LineageActors { get; }

    private MclslAnnualWorldSnapshot(IReadOnlyList<Actor> lineageActors)
    {
        LineageActors = lineageActors ?? EmptyActors;
    }

    internal static Builder BeginBuild(IReadOnlyList<Actor> lineageActors)
    {
        ReusableLineageActors.Clear();
        return new Builder(lineageActors, ReusableLineageActors);
    }

    internal sealed class Builder
    {
        private readonly IReadOnlyList<Actor> _actors;
        private readonly List<Actor> _result;
        private readonly int _sourceCount;
        private int _cursor;

        internal Builder(IReadOnlyList<Actor> actors, List<Actor> result)
        {
            _actors = actors;
            _result = result;
            _sourceCount = actors?.Count ?? 0;
        }

        internal bool Tick(int budget)
        {
            if (budget <= 0) return _cursor >= _sourceCount;
            int end = System.Math.Min(_sourceCount, _cursor + budget);
            long started = Stopwatch.GetTimestamp();
            double timeBudgetMs = MclslRuntimeWorkBudget.ScaleMilliseconds(0.65d, 0.20d);
            while (_cursor < end)
            {
                if (_cursor > 0 && (_cursor & 15) == 0
                    && (Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency >= timeBudgetMs)
                    break;
                Actor actor = _actors[_cursor++];
                if (!MclslHotPathPolicy.IsActorHotPathSafe(actor)) continue;
                if (!MclslEligibility.CanCultivate(actor)) continue;
                _result.Add(actor);
            }
            return _cursor >= _sourceCount;
        }

        internal MclslAnnualWorldSnapshot Complete() => new(_result);
    }
}
