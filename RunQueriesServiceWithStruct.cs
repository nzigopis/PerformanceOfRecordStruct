namespace PerformanceOfRecordStruct;

public class RunQueriesServiceWithStruct()
{
    public static IDictionary<string, RunAggregatesForInstrumentStruct> GetAggregatesFromDailyView(
        IQueryable<RunAggregates> aggregatesQuery)
    {
        var result = GetByInstrumentId(aggregatesQuery).
            GroupBy(a => a.InstrumentId).
            ToDictionary(g => g.Key, g => g.First());

        return result;
    }

    public static IEnumerable<RunAggregatesForInstrumentStruct> GetAggregatesFromMonthlyView(
        IQueryable<RunAggregates> monthlyAggregatesQuery,
        IQueryable<RunAggregates> excessDailyAtStartQuery,
        IQueryable<RunAggregates> excessDailyAtEndQuery)
    {

        var monthlyAggregates = GetByInstrumentId(monthlyAggregatesQuery);

        var excessDailyAggregatesAtStart =
            GetByInstrumentId(excessDailyAtStartQuery).
            ToLookup(a => a.InstrumentId);

        var excessDailyAggregatesAtEnd =
            GetByInstrumentId(excessDailyAtEndQuery).
            ToLookup(a => a.InstrumentId);

        var resultList = monthlyAggregates.Select(ma => ma
                                                        - excessDailyAggregatesAtStart[ma.InstrumentId].FirstOrDefault()
                                                        - excessDailyAggregatesAtEnd[ma.InstrumentId].FirstOrDefault()
            )
            .ToList();

        return resultList;
    }

    private static List<RunAggregatesForInstrumentStruct> GetByInstrumentId(IQueryable<RunAggregates> aggregatesQuery)
    {
        return aggregatesQuery.
            GroupBy(i => i.InstrumentId).
            Select(g => new RunAggregatesForInstrumentStruct(
                g.Key,
                g.Sum(r => r.CompletedSuccessfullyCount + r.CompletedWithWarningsCount + r.CompletedWithErrorsCount),
                g.Sum(r => r.CompletedSuccessfullyCount + r.CompletedWithWarningsCount),
                g.Sum(r => r.CompletedWithErrorsCount),
                g.Sum(r => r.ActiveTimeInMilliseconds),
                g.Sum(r => r.NVATimeInMilliseconds),
                g.Sum(r => r.ActiveTimeInMilliseconds) + g.Sum(r => r.NVATimeInMilliseconds)
                )
            ).ToList();
    }
}

public readonly record struct RunAggregatesForInstrumentStruct(
    string InstrumentId,
    long TotalRuns,
    int SuccessfulRuns,
    int FailedRuns,
    double ActiveTimeInMilliseconds,
    double NVATimeInMilliseconds,
    double TotalTimeInMilliseconds
)
{
    public static RunAggregatesForInstrumentStruct operator -
        (RunAggregatesForInstrumentStruct a1, RunAggregatesForInstrumentStruct? a2)
    {
        var dif = a2 == null
            ? a1
            : new RunAggregatesForInstrumentStruct(
                a1.InstrumentId,
                a1.TotalRuns - a2.Value.TotalRuns,
                a1.SuccessfulRuns - a2.Value.SuccessfulRuns,
                a1.FailedRuns - a2.Value.FailedRuns,
                a1.ActiveTimeInMilliseconds - a2.Value.ActiveTimeInMilliseconds,
                a1.NVATimeInMilliseconds - a2.Value.NVATimeInMilliseconds,
                a1.TotalTimeInMilliseconds - a2.Value.TotalTimeInMilliseconds
            );

        return dif;
    }
}
