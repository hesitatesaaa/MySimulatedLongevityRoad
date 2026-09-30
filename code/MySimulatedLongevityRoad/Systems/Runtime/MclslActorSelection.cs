using System;
using System.Collections.Generic;
using MySimulatedLongevityRoad.Core;

namespace MySimulatedLongevityRoad.Systems;

/// <summary>A resumable top-K query. Scores are evaluated once, and only K IDs are retained.</summary>
internal sealed class MclslActorSelection
{
    private readonly IReadOnlyList<Actor> _source;
    private readonly Func<Actor, bool> _predicate;
    private readonly Func<Actor, int> _score;
    private readonly int _sourceCount, _limit;
    private readonly List<long> _ids;
    private readonly List<int> _scores;
    private int _cursor;
    internal IReadOnlyList<Actor> Results { get; }
    internal MclslActorSelection(IReadOnlyList<Actor> source, int limit,
        Func<Actor, bool> predicate, Func<Actor, int> score)
    {
        if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit));
        _source = source; _sourceCount = source.Count; _limit = limit;
        _predicate = predicate; _score = score;
        _ids = new(limit); _scores = new(limit);
        Results = MclslActorRegistry.CreateView(_ids);
    }
    internal bool Tick(int maxActors = 64)
    {
        for (int n = 0; n < maxActors && _cursor < _sourceCount && !MclslFrameDeadline.Expired
            && !MclslAnnualFrameBudget.Expired; n++)
        {
            int position = _cursor++;
            Actor actor = position < _source.Count ? _source[position] : null;
            if (!MclslActorAccessor.Alive(actor) || (_predicate != null && !_predicate(actor))) continue;
            long id = MclslActorAccessor.Id(actor);
            int score = _score?.Invoke(actor) ?? 0;
            int low = 0, high = _ids.Count;
            while (low < high)
            {
                int middle = (low + high) / 2;
                if (_scores[middle] > score || (_scores[middle] == score && _ids[middle] < id)) low = middle + 1;
                else high = middle;
            }
            if (low >= _limit) continue;
            if (_ids.Count == _limit) { _ids.RemoveAt(_limit - 1); _scores.RemoveAt(_limit - 1); }
            _ids.Insert(low, id); _scores.Insert(low, score);
        }
        return _cursor >= _sourceCount;
    }
}
